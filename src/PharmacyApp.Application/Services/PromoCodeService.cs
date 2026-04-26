using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;

    public PromoCodeService(IUnitOfWorkRepository unitOfWork,  HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<PromoCodeDto>> CreatePromoCodeAsync(CreatePromoCodeDto createPromoCodeDto)
    {
        if (!Enum.TryParse<DiscountType>(createPromoCodeDto.DiscountType, ignoreCase: true, out var discountType))
            return Result<PromoCodeDto>.Conflict(
                $"Invalid discount type: '{createPromoCodeDto.DiscountType}'." +
                $" Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");
        
        var codeExists = await _unitOfWork.PromoCodes.CodeExistsAsync(createPromoCodeDto.Code.ToUpper());
        if (codeExists)
            throw new BadRequestException($"Promo code '{createPromoCodeDto.Code}' already exists.");

        var promoCode = new PromoCode(createPromoCodeDto.Code, createPromoCodeDto.Description, discountType,
            createPromoCodeDto.Value, createPromoCodeDto.StartDate, createPromoCodeDto.EndDate, 
            createPromoCodeDto.ApplicableToAllProducts, createPromoCodeDto.MaxUsageCount, createPromoCodeDto.MaxUsagePerUser,
            createPromoCodeDto.MinimumOrderAmount, createPromoCodeDto.MaximumDiscountAmount);

        promoCode.UpdateApplicableProducts(
            createPromoCodeDto.ProductIds,
            createPromoCodeDto.CategoryIds);

        var createdPromoCode = await _unitOfWork.PromoCodes.AddAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();
        await InvalidatePromoCodeCachesAsync(createdPromoCode.PromoCodeId, createdPromoCode.Code);
        return  Result<PromoCodeDto>.Success(createdPromoCode.ToPromoCodeDto());
    }

    public async Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.PromoCodes.ById(promoCodeId),
            async _ =>
            {
                var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
                return promoCode?.ToPromoCodeDto();
            });
    }

    public async Task<PromoCodeDto?> GetPromoCodeByCodeAsync(string code)
    {
        return await _cache.GetOrCreateAsync(CacheKeys.PromoCodes.ByCode(code.ToUpper()),
            async _ =>
            {
                var promoCode = await _unitOfWork.PromoCodes.GetByCodeAsync(code.ToUpper());
                return promoCode?.ToPromoCodeDto();
            });
    }

    public async Task<IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.PromoCodes.All, async _ =>
        {
            var promoCodes = await _unitOfWork.PromoCodes.GetAllAsync();
            return promoCodes.Select(p => p.ToPromoCodeDto());
        }); 
    }

    public async Task<IEnumerable<PromoCodeDto>> GetActivePromoCodesAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.PromoCodes.Active, async _ =>
        {
            var promoCodes = await _unitOfWork.PromoCodes.GetActivePromoCodesAsync();
            return promoCodes.Select(p => p.ToPromoCodeDto());
        });
    }

    public async Task<Result> UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto updatePromoCodeDto)
    {
        if (!Enum.TryParse<DiscountType>(updatePromoCodeDto.DiscountType, ignoreCase: true, out var discountType))
            return Result.Conflict(
                $"Invalid discount type: '{updatePromoCodeDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");
        
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);

        if (promoCode is null)
            return Result.NotFound("Promo code not found.");

        var previousCode = promoCode.Code;

        // Check if the new code (if changed) is unique
        var codeExists = await _unitOfWork.PromoCodes.CodeExistsAsync(updatePromoCodeDto.Code.ToUpper(), promoCodeId);
        if (codeExists)
            return Result.Conflict($"Promo code '{updatePromoCodeDto.Code}' already exists.");
        
        promoCode.Update(updatePromoCodeDto.Code.ToUpper(), updatePromoCodeDto.Description, discountType,
            updatePromoCodeDto.Value, updatePromoCodeDto.StartDate, updatePromoCodeDto.EndDate, updatePromoCodeDto.ApplicableToAllProducts,
            updatePromoCodeDto.MaxUsageCount, updatePromoCodeDto.MaxUsagePerUser, updatePromoCodeDto.MinimumOrderAmount,
            updatePromoCodeDto.MaximumDiscountAmount
        );

        promoCode.UpdateApplicableProducts(
            updatePromoCodeDto.ProductIds,   
            updatePromoCodeDto.CategoryIds);
        
        await _unitOfWork.PromoCodes.UpdateAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();

        await InvalidatePromoCodeCachesAsync(promoCodeId, previousCode, promoCode.Code);
        
        return Result.Success();
    }

    public async Task<Result> DeletePromoCodeAsync(Guid promoCodeId)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
        if (promoCode is null)
            return Result.NotFound("Promo code not found.");
        
        await _unitOfWork.PromoCodes.DeleteAsync(promoCodeId);
        await _unitOfWork.SaveChangesAsync();

        await InvalidatePromoCodeCachesAsync(promoCodeId, promoCode.Code);
        
        return Result.Success();
    }
    
    public async Task<Result> ActivatePromoCodeAsync(Guid promoCodeId)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
        if (promoCode is null)
            return Result.NotFound("Promo code not found.");

        promoCode.Activate(); 

        await _unitOfWork.PromoCodes.UpdateAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();
        await InvalidatePromoCodeCachesAsync(promoCodeId, promoCode.Code);
        return Result.Success();
    }

    public async Task<Result> DeactivatePromoCodeAsync(Guid promoCodeId)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
        if (promoCode is null)
            return Result.NotFound("Promo code not found.");

        promoCode.Deactivate();

        await _unitOfWork.PromoCodes.UpdateAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();
        await InvalidatePromoCodeCachesAsync(promoCodeId, promoCode.Code);
        return Result.Success();
    }

    public async Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(PromoCodeValidationResults validationResults)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByCodeAsync(validationResults.Code);
        var categoryIds = validationResults.CategoryIds;

        if (promoCode is null)
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = "Promo code not found."
            };

        if (!promoCode.IsValid())
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = "Promo code is expired or inactive."
            };

        if (!promoCode.CanBeUsedByUser(validationResults.UserId))
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = "You have exceeded the maximum usage limit for this promo code."
            };

        if (promoCode.MinimumOrderAmount.HasValue && validationResults.OrderAmount < promoCode.MinimumOrderAmount.Value)
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = $"Minimum order amount of {promoCode.MinimumOrderAmount:C} is required."
            };

        // Check if promo code is applicable to specific products
        if (!promoCode.ApplicableToAllProducts)
        {
            if (categoryIds.Count == 0 && validationResults.ProductIds.Count > 0)
            {
                var products = await _unitOfWork.Products.GetByIdsAsync(validationResults.ProductIds);
                categoryIds = products
                    .Select(product => product.CategoryId)
                    .Distinct()
                    .ToList();
            }

            var hasApplicableProducts = validationResults.ProductIds.Any(pid =>
                promoCode.PromoCodeProducts.Any(p => p.ProductId == pid));

            var matchesCategory = categoryIds.Any(cid =>
                promoCode.PromoCodeCategories.Any(c => c.CategoryId == cid));

            if (!hasApplicableProducts && !matchesCategory)
                return new PromoCodeValidationResultDto
                {
                    IsValid = false,
                    Message = "Promo code is not applicable to the items in your cart."
                };
        }

        // Calculate discount amount
        var discountAmount = CalculateDiscount(promoCode, validationResults.OrderAmount);

        return new PromoCodeValidationResultDto
        {
            IsValid = true,
            Message = "Promo code is valid.",
            DiscountAmount = discountAmount,
            PromoCodeId = promoCode.PromoCodeId
        };
    }

    private static decimal CalculateDiscount(PromoCode promoCode, decimal orderAmount)
    {
        decimal discount = promoCode.DiscountType == DiscountType.Percentage
            ? orderAmount * (promoCode.Value / 100)
            : promoCode.Value;

        if (promoCode.MaximumDiscountAmount.HasValue && discount > promoCode.MaximumDiscountAmount.Value)
            discount = promoCode.MaximumDiscountAmount.Value;

        return discount;
    }

    private async Task InvalidatePromoCodeCachesAsync(Guid promoCodeId, params string[] codes)
    {
        await _cache.RemoveAsync(CacheKeys.PromoCodes.All);
        await _cache.RemoveAsync(CacheKeys.PromoCodes.Active);
        await _cache.RemoveAsync(CacheKeys.PromoCodes.ById(promoCodeId));

        var processedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
        {
            if (string.IsNullOrWhiteSpace(code))
                continue;

            var normalizedCode = code.Trim().ToUpperInvariant();
            if (!processedCodes.Add(normalizedCode))
                continue;

            await _cache.RemoveAsync(CacheKeys.PromoCodes.ByCode(normalizedCode));
        }
    }
}
