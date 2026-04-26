using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Bonus;
using PharmacyApp.Application.Contracts.Bonus.Admin;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities.Bonus;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class BonusService : IBonusService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;

    public BonusService(IUnitOfWorkRepository unitOfWork, HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    // User

    public async Task<BonusAccountDto> GetOrCreateAccountAsync(string userId)
    {
        var account = await GetOrCreateAsync(userId);
        return account.ToBonusAccountDto();
    }

    public async Task<PaginatedList<BonusTransactionDto>> GetTransactionsAsync(
        string userId,
        QueryParams queryParams)
    {
        var normalizedQuery = queryParams with
        {
            PageIndex = Math.Max(1, queryParams.PageIndex),
            PageSize = Math.Clamp(queryParams.PageSize, 1, 100)
        };

        var transactions = await _unitOfWork.Bonuses.GetTransactionsAsync(userId, normalizedQuery);

        return new PaginatedList<BonusTransactionDto>
        {
            Items = transactions.Items.Select(t => t.ToBonusTransactionDto()).ToList(),
            TotalCount = transactions.TotalCount,
            PageIndex = transactions.PageIndex,
            PageSize = transactions.PageSize,
            TotalPages = transactions.TotalPages
        };
    }

    public async Task<decimal> EarnPointsAsync(string userId, int orderId, decimal paidAmount)
    {
        var settings = await GetSettingsAsync();

        if (!settings.IsEarningEnabled)
            return 0;

        if (paidAmount < settings.MinOrderAmountToEarn)
            return 0;

        var earned = Math.Round(paidAmount * settings.EarningRate, 2);
        if (earned <= 0)
            return 0;

        var account = await GetOrCreateAsync(userId);
        var earnTransaction = account.EarnForOrder(orderId, earned);
        await _unitOfWork.Bonuses.UpdateAsync(account);

        await _unitOfWork.Bonuses.AddTransactionAsync(earnTransaction);
        await _unitOfWork.SaveChangesAsync();

        return earned;
    }

    public async Task<Result<decimal>> RedeemPointsAsync(string userId, int orderId, decimal pointsToRedeem)
    {
        if (pointsToRedeem <= 0)
            return Result<decimal>.BadRequest("Points to redeem must be greater than 0.");

        if (orderId <= 0)
            return Result<decimal>.BadRequest("Order ID must be greater than 0 for bonus redemption.");

        var settings = await GetSettingsAsync();

        if (!settings.IsRedemptionEnabled)
            return Result<decimal>.BadRequest("Bonus redemption is currently disabled.");

        var account = await GetOrCreateAsync(userId);
        var discount = Math.Round(pointsToRedeem, 2);

        try
        {
            var redeemTransaction = account.RedeemForOrder(orderId, pointsToRedeem, discount);
            await _unitOfWork.Bonuses.UpdateAsync(account);

            await _unitOfWork.Bonuses.AddTransactionAsync(redeemTransaction);
            await _unitOfWork.SaveChangesAsync();

            return Result<decimal>.Success(discount);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result<decimal>.BadRequest(ex.Message);
        }
    }

    public async Task ReverseOrderBonusesAsync(string userId, int orderId)
    {
        var account = await _unitOfWork.Bonuses.GetByUserIdAsync(userId);
        if (account is null) return;

        var transactions = (await _unitOfWork.Bonuses.GetTransactionsByOrderIdAsync(orderId))
            .Where(t => t.BonusAccountId == account.Id)
            .ToList();

        if (transactions.Count == 0)
            return;

        var refundTransactions = account.ReverseOrderTransactions(orderId, transactions);
        if (refundTransactions.Count == 0)
            return;

        await _unitOfWork.Bonuses.UpdateAsync(account);

        foreach (var refundTransaction in refundTransactions)
            await _unitOfWork.Bonuses.AddTransactionAsync(refundTransaction);

        await _unitOfWork.SaveChangesAsync();
    }

    // Admin: accounts

    public async Task<IEnumerable<BonusAccountDto>> GetAllAccountsAsync(int pageIndex = 1, int pageSize = 20)
    {
        var accounts = await _unitOfWork.Bonuses.GetAllAccountsAsync(pageIndex, pageSize);
        return accounts.Select(a => a.ToBonusAccountDto());
    }

    public async Task<Result> AdminAdjustAsync(string userId, AdjustBonusDto dto)
    {
        var account = await GetOrCreateAsync(userId);
        try
        {
            var adjustmentTransaction = account.ApplyAdminAdjustment(dto.Points, dto.Reason);
            await _unitOfWork.Bonuses.UpdateAsync(account);
            await _unitOfWork.Bonuses.AddTransactionAsync(adjustmentTransaction);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.BadRequest(ex.Message);
        }
    }

    public async Task<BonusSettingsDto> GetSettingsAsync()
        => await _cache.GetOrCreateAsync(CacheKeys.Bonus.Settings, async _ =>
        {
            var settings = await _unitOfWork.Bonuses.GetSettingsAsync();
            return settings.ToBonusSettingsDto();
        }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(30) });

    public async Task<BonusSettingsDto> UpdateSettingsAsync(UpdateBonusSettingsDto dto)
    {
        var settings = await _unitOfWork.Bonuses.GetSettingsAsync();

        settings.EarningRate = dto.EarningRate;
        settings.MinOrderAmountToEarn = dto.MinOrderAmountToEarn;
        settings.MaxRedeemPercent = dto.MaxRedeemPercent;
        settings.IsEarningEnabled = dto.IsEarningEnabled;
        settings.IsRedemptionEnabled = dto.IsRedemptionEnabled;

        await _unitOfWork.Bonuses.UpdateSettingsAsync(settings);
        await _unitOfWork.SaveChangesAsync();
        await _cache.RemoveAsync(CacheKeys.Bonus.Settings);
        return settings.ToBonusSettingsDto();
    }

    // Check if a user exists
    private async Task<BonusAccount> GetOrCreateAsync(string userId)
    {
        var account = await _unitOfWork.Bonuses.GetByUserIdAsync(userId);
        if (account is null)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException("User not found.");

            account = new BonusAccount(Guid.NewGuid(), userId, 0);
            await _unitOfWork.Bonuses.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }
        return account;
    }
}
