using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Bonus;
using PharmacyApp.Application.Contracts.Bonus.Admin;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities.Bonus;
using PharmacyApp.Domain.Enums;
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

    public async Task<IEnumerable<BonusTransactionDto>> GetTransactionsAsync(
        string userId, int pageIndex = 1, int pageSize = 20)
    {
        var transactions = await _unitOfWork.Bonuses.GetTransactionsAsync(userId, pageIndex, pageSize);
        return transactions.Select(t => t.ToBonusTransactionDto());
    }

    public async Task<decimal> EarnPointsAsync(string userId, int orderId, decimal paidAmount)
    {
        var settings = await GetSettingsAsync();

        if (!settings.IsEarningEnabled)
            return 0;

        if (paidAmount < settings.MinOrderAmountToEarn)
            return 0;

        var account = await GetOrCreateAsync(userId);
        var earned = Math.Round(paidAmount * settings.EarningRate, 2);

        account.Balance += earned;
        await _unitOfWork.Bonuses.UpdateAsync(account);

        await _unitOfWork.Bonuses.AddTransactionAsync(new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = account.Id,
            Type = BonusTransactionType.Earned,
            Points = earned,
            Description = $"Earned {earned} points for order #{orderId}.",
            OrderId = orderId
        });

        return earned;
    }

    public async Task<Result<decimal>> RedeemPointsAsync(string userId, int orderId, decimal pointsToRedeem)
    {
        if (pointsToRedeem <= 0)
            return Result<decimal>.BadRequest("Points to redeem must be greater than 0.");

        var settings = await GetSettingsAsync();

        if (!settings.IsRedemptionEnabled)
            return Result<decimal>.BadRequest("Bonus redemption is currently disabled.");

        var account = await GetOrCreateAsync(userId);

        if (pointsToRedeem > account.Balance)
            return Result<decimal>.BadRequest(
                $"Not enough points. Available: {account.Balance}, requested: {pointsToRedeem}.");

        var discount = Math.Round(pointsToRedeem, 2);

        account.Balance -= pointsToRedeem;
        await _unitOfWork.Bonuses.UpdateAsync(account);

        await _unitOfWork.Bonuses.AddTransactionAsync(new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = account.Id,
            Type = BonusTransactionType.Redeemed,
            Points = pointsToRedeem,
            Description = $"Used {pointsToRedeem} points for order #{orderId} discount (−{discount}.)",
            OrderId = orderId
        });

        return Result<decimal>.Success(discount);
    }

    public async Task ReverseOrderBonusesAsync(string userId, int orderId)
    {
        var account = await _unitOfWork.Bonuses.GetByUserIdAsync(userId);
        if (account is null) return;

        var transactions = await _unitOfWork.Bonuses.GetTransactionsByOrderIdAsync(orderId);

        foreach (var tx in transactions)
        {
            account.Balance += tx.Type switch
            {
                BonusTransactionType.Earned => -tx.Points,
                BonusTransactionType.Redeemed => tx.Points,
                _ => 0
            };
        }

        account.Balance = Math.Max(0, account.Balance);
        await _unitOfWork.Bonuses.UpdateAsync(account);

        foreach (var tx in transactions)
        {
            await _unitOfWork.Bonuses.AddTransactionAsync(new BonusTransaction
            {
                Id = Guid.NewGuid(),
                BonusAccountId = account.Id,
                Type = BonusTransactionType.Refunded,
                Points = tx.Points,
                Description = $"Refunded ({tx.Type}) for order #{orderId} cancellation.",
                OrderId = orderId
            });
        }
    }

    // Admin: accounts 

    public async Task<IEnumerable<BonusAccountDto>> GetAllAccountsAsync(int pageIndex = 1, int pageSize = 20)
    {
        var accounts = await _unitOfWork.Bonuses.GetAllAccountsAsync(pageIndex, pageSize);
        return accounts.Select(a => a.ToBonusAccountDto());
    }

    public async Task<Result> AdminAdjustAsync(string userId, AdjustBonusDto dto)
    {
        if (dto.Points == 0)
            return Result.Failure("Points cannot be 0.", 400);

        var account = await GetOrCreateAsync(userId);

        if (account.Balance + dto.Points < 0)
            return Result.Failure($"Adjustment would result in negative balance. Current: {account.Balance}, delta: {dto.Points}.", 400);

        account.Balance += dto.Points;
        await _unitOfWork.Bonuses.UpdateAsync(account);

        await _unitOfWork.Bonuses.AddTransactionAsync(new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = account.Id,
            Type = BonusTransactionType.AdminAdjustment,
            Points = dto.Points,
            Description = $"Admin adjustment: {dto.Reason}"
        });
        
        return Result.Success();
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
        }
        return account;
    }
}