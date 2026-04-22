using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Discount;
using PharmacyApp.Application.Contracts.Promotion;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Enums;
using System.Text;

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
            createDiscountDto.MinimumOrderAmount, createDiscountDto.MaximumOrderAmount,
            createDiscountDto.IsActive
        );

        discount.ProductDiscounts = createDiscountDto.ProductIds
            .Select(pid => new ProductDiscount { ProductId = pid, DiscountId = discount.DiscountId })
            .ToList();

        discount.CategoryDiscounts = createDiscountDto.CategoryIds
            .Select(cid => new CategoryDiscount { CategoryId = cid, DiscountId = discount.DiscountId })
            .ToList();

        var createdDiscount = await _unitOfWork.Discounts.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Discounts.All);
        await _cache.RemoveAsync(CacheKeys.Discounts.Active);
        await _cache.RemoveAsync(CacheKeys.Discounts.ActiveEntities);

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

    public async Task<IReadOnlyCollection<PromotionListItemDto>> GetActivePromotionsAsync()
    {
        var now = DateTime.UtcNow;
        var discounts = await GetActiveDiscountsAsync();

        return discounts
            .Where(discount => discount.IsActive && now >= discount.StartDate && now <= discount.EndDate)
            .OrderByDescending(discount => discount.StartDate)
            .Select(discount => new PromotionListItemDto
            {
                DiscountId = discount.DiscountId,
                Slug = BuildPromotionSlug(discount.DiscountId, discount.Name),
                Name = discount.Name,
                Description = discount.Description,
                DiscountType = discount.DiscountType,
                Value = discount.Value,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                ProductTargetsCount = discount.ProductIds.Distinct().Count(),
                CategoryTargetsCount = discount.CategoryIds.Distinct().Count()
            })
            .ToList();
    }

    public async Task<Result<PromotionDetailsDto>> GetActivePromotionBySlugAsync(string slug)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
            return Result<PromotionDetailsDto>.BadRequest("Promotion slug is required.");

        if (!TryExtractDiscountId(normalizedSlug, out var discountId))
            return Result<PromotionDetailsDto>.NotFound("Promotion not found.");

        var discount = await GetDiscountByIdAsync(discountId);
        if (discount is null || !IsCurrentlyActive(discount))
            return Result<PromotionDetailsDto>.NotFound("Promotion not found.");

        var productIds = discount.ProductIds.Distinct().ToHashSet();
        var categoryIds = discount.CategoryIds.Distinct().ToHashSet();

        var products = await _unitOfWork.Products.GetAllAsync()
            .Where(product => productIds.Contains(product.Id) || categoryIds.Contains(product.CategoryId))
            .ToListAsync();

        Dictionary<int, decimal> discountedPrices = [];
        if (products.Count > 0)
        {
            discountedPrices = await CalculateDiscountedPricesAsync(
                products
                    .Select(product => new ProductPriceContext(product.Id, product.CategoryId, product.Price))
                    .ToList());
        }

        var productDtos = products
            .OrderBy(product => product.Name)
            .Select(product => product.ToProductDto(
                discountedPrices.TryGetValue(product.Id, out var discountedPrice)
                    ? discountedPrice
                    : product.Price))
            .ToList();

        return Result<PromotionDetailsDto>.Success(new PromotionDetailsDto
        {
            DiscountId = discount.DiscountId,
            Slug = BuildPromotionSlug(discount.DiscountId, discount.Name),
            Name = discount.Name,
            Description = discount.Description,
            DiscountType = discount.DiscountType,
            Value = discount.Value,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            ProductTargetsCount = productIds.Count,
            CategoryTargetsCount = categoryIds.Count,
            Products = productDtos
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
            updateDiscountDto.MinimumOrderAmount, updateDiscountDto.MaximumOrderAmount,
            updateDiscountDto.IsActive);
        
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
        await _cache.RemoveAsync(CacheKeys.Discounts.ActiveEntities);
        await _cache.RemoveAsync(CacheKeys.Discounts.ById(discountId));
        
        return Result.Success();
    }

    public async Task<Result> DeleteDiscountAsync(Guid discountId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);

        if (discount is null)
            return Result.NotFound($"Discount {discountId} not found.");

        await _unitOfWork.Discounts.DeleteAsync(discountId);
        await _unitOfWork.SaveChangesAsync();
        
        await _cache.RemoveAsync(CacheKeys.Discounts.All);
        await _cache.RemoveAsync(CacheKeys.Discounts.Active);
        await _cache.RemoveAsync(CacheKeys.Discounts.ActiveEntities);
        await _cache.RemoveAsync(CacheKeys.Discounts.ById(discountId));
        
        return  Result.Success();
    }
    
    public async Task<decimal> CalculateDiscountedPriceAsync(int productId, int categoryId, decimal originalPrice)
    {
        var prices = await CalculateDiscountedPricesAsync(
            new[] { new ProductPriceContext(productId, categoryId, originalPrice) });

        return prices[productId];
    }

    public async Task<Dictionary<int, decimal>> CalculateDiscountedPricesAsync(IReadOnlyCollection<ProductPriceContext> products)
    {
        var activeDiscounts = await _cache.GetOrCreateAsync(
            CacheKeys.Discounts.ActiveEntities,
            async _ =>
            {
                var discounts = await _unitOfWork.Discounts.GetActiveDiscountsAsync();
                return discounts
                    .Select(d => d.ToActiveDiscountSnapshot())
                    .ToList();
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) });
        
        var now = DateTime.UtcNow;
        
        var byProduct = activeDiscounts
            .SelectMany(d => d.ProductIds.Select(productId => new { ProductId = productId, Discount = d }))
            .GroupBy(p => p.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Discount).ToList());
        
        var byCategory = activeDiscounts
            .SelectMany(d => d.CategoryIds.Select(categoryId => new { CategoryId = categoryId, Discount = d }))
            .GroupBy(c => c.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Discount).ToList());

        var result = new Dictionary<int, decimal>(products.Count);

        foreach (var p in products)
        {
            byProduct.TryGetValue(p.ProductId, out var productDiscounts);
            byCategory.TryGetValue(p.CategoryId, out var categoryDiscounts);
            
            var best = (productDiscounts ?? Enumerable.Empty<ActiveDiscountSnapshot>())
                .Concat(categoryDiscounts ?? Enumerable.Empty<ActiveDiscountSnapshot>())
                .DistinctBy(d => d.DiscountId)
                .Where(d => d.IsActive && now >= d.StartDate && now <= d.EndDate)
                .Select(d => d.DiscountType == DiscountType.Percentage
                    ? p.OriginalPrice * d.Value / 100m
                    : d.Value)
                .DefaultIfEmpty(0m)
                .Max();
            
            result[p.ProductId] = Math.Round(p.OriginalPrice - best, 2);
        }
        return result;
    }

    private static bool IsCurrentlyActive(DiscountDto discount)
    {
        var now = DateTime.UtcNow;
        return discount.IsActive && now >= discount.StartDate && now <= discount.EndDate;
    }

    private static string NormalizeSlug(string slug)
    {
        return slug?.Trim() ?? string.Empty;
    }

    private static string BuildPromotionSlug(Guid discountId, string name)
    {
        var nameSlug = ToSlug(name);
        return string.IsNullOrWhiteSpace(nameSlug)
            ? $"promotion-{discountId:N}"
            : $"{nameSlug}-{discountId:N}";
    }

    private static bool TryExtractDiscountId(string slug, out Guid discountId)
    {
        discountId = Guid.Empty;

        if (Guid.TryParse(slug, out discountId))
            return true;

        var lastDashIndex = slug.LastIndexOf('-');
        if (lastDashIndex < 0 || lastDashIndex == slug.Length - 1)
            return false;

        var candidate = slug[(lastDashIndex + 1)..];

        return Guid.TryParseExact(candidate, "N", out discountId)
            || Guid.TryParse(candidate, out discountId);
    }

    private static string ToSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        var previousIsDash = false;

        foreach (var symbol in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(symbol))
            {
                builder.Append(symbol);
                previousIsDash = false;
                continue;
            }

            if (previousIsDash)
                continue;

            builder.Append('-');
            previousIsDash = true;
        }

        return builder.ToString().Trim('-');
    }
}
