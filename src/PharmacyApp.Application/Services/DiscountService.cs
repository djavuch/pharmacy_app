using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Discount;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Enums;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;

    public DiscountService(IUnitOfWorkRepository unitOfWork,  HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<DiscountDto>> CreateDiscountAsync(CreateDiscountDto createDiscountDto)
    {
        if (!Enum.TryParse<DiscountType>(createDiscountDto.DiscountType, ignoreCase: true, out var discountType))
            return Result<DiscountDto>.Conflict(
                $"Invalid discount type: '{createDiscountDto.DiscountType}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");
        
        var discount = new Discount(createDiscountDto.Name, createDiscountDto.Description,
            Enum.Parse<DiscountType>(createDiscountDto.DiscountType), createDiscountDto.Value,
            DateTime.SpecifyKind(createDiscountDto.StartDate, DateTimeKind.Utc),
            DateTime.SpecifyKind(createDiscountDto.EndDate, DateTimeKind.Utc),
            createDiscountDto.MinimumOrderAmount, createDiscountDto.MaximumOrderAmount
        );

        discount.ProductDiscounts = createDiscountDto.ProductIds
            .Select(pid => new ProductDiscount { ProductId = pid, DiscountId = discount.DiscountId })
            .ToList();

        discount.CategoryDiscounts = createDiscountDto.CategoryIds
            .Select(cid => new CategoryDiscount { CategoryId = cid, DiscountId = discount.DiscountId })
            .ToList();

        var createdDiscount = await _unitOfWork.Discounts.AddAsync(discount);
        return Result<DiscountDto>.Success(createdDiscount.ToDiscountDto());
    }

    public async Task<DiscountDto?> GetDiscountByIdAsync(Guid discountId)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.ById(discountId),
            async _ =>
            {
                var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);
                return discount?.ToDiscountDto();
            });
    }

    public async Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync()
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.All,
            async _ =>
            {
                var discounts = await _unitOfWork.Discounts.GetAllAsync();
                return discounts.Select(d => d.ToDiscountDto()).ToList();
            });
    }

    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync()
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.Active,
            async _ =>
            {
                var discounts = await _unitOfWork.Discounts.GetActiveDiscountsAsync();
                return discounts.Select(d => d.ToDiscountDto()).ToList();
            });
    }

    public async Task<Result> UpdateDiscountAsync(Guid discountId, UpdateDiscountDto updateDiscountDto)
    {
        if (!Enum.TryParse<DiscountType>(updateDiscountDto.DiscountType, ignoreCase: true, out var discountType))
            return Result.Conflict(
                $"Invalid discount type: '{updateDiscountDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");

        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);

        if (discount is null)
            return Result.NotFound("Discount not found");
        
        discount.Update(updateDiscountDto.Name, updateDiscountDto.Description, 
            discountType, updateDiscountDto.Value, 
            DateTime.SpecifyKind(updateDiscountDto.StartDate, DateTimeKind.Utc),
            DateTime.SpecifyKind(updateDiscountDto.EndDate, DateTimeKind.Utc), 
            updateDiscountDto.MinimumOrderAmount, updateDiscountDto.MaximumOrderAmount);
        
        discount.ProductDiscounts.Clear();
        foreach (var pid in updateDiscountDto.ProductIds)
        {
            discount.ProductDiscounts.Add(new ProductDiscount { ProductId = pid, DiscountId = discountId });
        }

        discount.CategoryDiscounts.Clear();
        foreach (int cid in updateDiscountDto.CategoryIds)
        {
            discount.CategoryDiscounts.Add(new CategoryDiscount { CategoryId = cid, DiscountId = discountId });
        }

        await _unitOfWork.Discounts.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync();
        
        await _cache.RemoveAsync(CacheKeys.Discounts.All);
        await _cache.RemoveAsync(CacheKeys.Discounts.Active);
        await _cache.RemoveAsync(CacheKeys.Discounts.ById(discountId));
        
        return Result.Success();
    }

    public async Task<Result> DeleteDiscountAsync(Guid discountId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);

        if (discount is null)
            return Result.NotFound($"Discount {discountId} not found.");

        await _unitOfWork.Discounts.DeleteAsync(discountId);
        
        await _cache.RemoveAsync(CacheKeys.Discounts.All);
        await _cache.RemoveAsync(CacheKeys.Discounts.Active);
        await _cache.RemoveAsync(CacheKeys.Discounts.ById(discountId));
        
        return  Result.Success();
    }

    public async Task<decimal> CalculateDiscountedPriceAsync(int productId, int categoryId, decimal originalPrice)
    {
        var productDiscounts = await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.ByProduct(productId), async _ =>
                (await _unitOfWork.Discounts.GetDiscountsByProductIdAsync(productId)).ToList(),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) });

        var categoryDiscounts = await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.ByCategory(categoryId), async _ =>
                (await _unitOfWork.Discounts.GetDiscountsByCategoryIdAsync(categoryId)).ToList(),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) });

        var activeDiscount = productDiscounts
            .Concat(categoryDiscounts)
            .DistinctBy(d => d.DiscountId)
            .Where(d => d.isValid())
            .OrderByDescending(d => CalculateAmount(d, originalPrice))
            .FirstOrDefault();

        if (activeDiscount is null)
            return originalPrice;

        var discountedPrice = activeDiscount.DiscountType == DiscountType.Percentage
        ? originalPrice - (originalPrice * activeDiscount.Value / 100)
        : originalPrice - activeDiscount.Value;

        return Math.Round(discountedPrice, 2);
    }

    private decimal CalculateAmount(Discount discount, decimal price)
    {
        var amount = discount.DiscountType == DiscountType.Percentage
            ? price * discount.Value / 100
            : discount.Value;

        return Math.Round(amount, 2);
    }
}
