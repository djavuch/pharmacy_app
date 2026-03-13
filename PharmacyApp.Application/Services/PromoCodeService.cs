using PharmacyApp.Application.DTOs.PromoCode;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly IUnitOfWorkRepository _unitOfWork;

    public PromoCodeService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeDto createPromoCodeDto)
    {
        if (!Enum.TryParse<DiscountType>(createPromoCodeDto.DiscountType, ignoreCase: true, out var discountType))
        {
            throw new BadRequestException(
                $"Invalid discount type: '{createPromoCodeDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");
        }

        // Проверка уникальности кода
        var codeExists = await _unitOfWork.PromoCodes.CodeExistsAsync(createPromoCodeDto.Code.ToUpper());
        if (codeExists)
        {
            throw new BadRequestException($"Promo code '{createPromoCodeDto.Code}' already exists.");
        }

        var promoCode = new PromoCodeModel
        {
            PromoCodeId = Guid.NewGuid(),
            Code = createPromoCodeDto.Code.ToUpper(),
            Description = createPromoCodeDto.Description,
            DiscountType = discountType,
            Value = createPromoCodeDto.Value,
            StartDate = createPromoCodeDto.StartDate,
            EndDate = createPromoCodeDto.EndDate,
            IsActive = createPromoCodeDto.IsActive,
            MaxUsageCount = createPromoCodeDto.MaxUsageCount,
            MaxUsagePerUser = createPromoCodeDto.MaxUsagePerUser,
            MinimumOrderAmount = createPromoCodeDto.MinimumOrderAmount,
            MaximumDiscountAmount = createPromoCodeDto.MaximumDiscountAmount,
            ApplicableToAllProducts = createPromoCodeDto.ApplicableToAllProducts
        };

        promoCode.ValidateBusinessRules();

        if (!promoCode.ApplicableToAllProducts)
        {
            promoCode.PromoCodeProducts = (createPromoCodeDto.ProductIds ?? [])
                .Select(pid => new PromoCodeProductModel { ProductId = pid, PromoCodeId = promoCode.PromoCodeId })
                .ToList();

            promoCode.PromoCodeCategories = (createPromoCodeDto.CategoryIds ?? [])
                .Select(cid => new PromoCodeCategoryModel { CategoryId = cid, PromoCodeId = promoCode.PromoCodeId })
                .ToList();
        }

        var createdPromoCode = await _unitOfWork.PromoCodes.AddAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();
        return createdPromoCode.ToPromoCodeDto();
    }

    public async Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
        return promoCode?.ToPromoCodeDto();
    }

    public async Task<PromoCodeDto?> GetPromoCodeByCodeAsync(string code)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByCodeAsync(code);
        return promoCode?.ToPromoCodeDto();
    }

    public async Task<IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync()
    {
        var promoCodes = await _unitOfWork.PromoCodes.GetAllAsync();
        return promoCodes.Select(p => p.ToPromoCodeDto());
    }

    public async Task<IEnumerable<PromoCodeDto>> GetActivePromoCodesAsync()
    {
        var promoCodes = await _unitOfWork.PromoCodes.GetActivePromoCodesAsync();
        return promoCodes.Select(p => p.ToPromoCodeDto());
    }

    public async Task UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto updatePromoCodeDto)
    {
        if (!Enum.TryParse<DiscountType>(updatePromoCodeDto.DiscountType, ignoreCase: true, out var discountType))
        {
            throw new BadRequestException(
                $"Invalid discount type: '{updatePromoCodeDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");
        }

        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);

        if (promoCode is null)
        {
            throw new NotFoundException("Promo code not found.");
        }

        // Check if the new code (if changed) is unique
        var codeExists = await _unitOfWork.PromoCodes.CodeExistsAsync(updatePromoCodeDto.Code.ToUpper(), promoCodeId);
        if (codeExists)
        {
            throw new BadRequestException($"Promo code '{updatePromoCodeDto.Code}' already exists.");
        }

        promoCode.Code = updatePromoCodeDto.Code.ToUpper();
        promoCode.Description = updatePromoCodeDto.Description;
        promoCode.DiscountType = discountType;
        promoCode.Value = updatePromoCodeDto.Value;
        promoCode.StartDate = updatePromoCodeDto.StartDate;
        promoCode.EndDate = updatePromoCodeDto.EndDate;
        promoCode.IsActive = updatePromoCodeDto.IsActive;
        promoCode.MaxUsageCount = updatePromoCodeDto.MaxUsageCount;
        promoCode.MaxUsagePerUser = updatePromoCodeDto.MaxUsagePerUser;
        promoCode.MinimumOrderAmount = updatePromoCodeDto.MinimumOrderAmount;
        promoCode.MaximumDiscountAmount = updatePromoCodeDto.MaximumDiscountAmount;
        promoCode.ApplicableToAllProducts = updatePromoCodeDto.ApplicableToAllProducts;

        promoCode.ValidateBusinessRules();

        promoCode.PromoCodeProducts.Clear();
        promoCode.PromoCodeCategories.Clear();

        if (!promoCode.ApplicableToAllProducts)
        {
            foreach (var pid in updatePromoCodeDto.ProductIds ?? [])
            {
                promoCode.PromoCodeProducts.Add(new PromoCodeProductModel { ProductId = pid, PromoCodeId = promoCodeId });
            }

            foreach (var cid in updatePromoCodeDto.CategoryIds ?? [])
            {
                promoCode.PromoCodeCategories.Add(new PromoCodeCategoryModel { CategoryId = cid, PromoCodeId = promoCodeId });
            }
        }

        await _unitOfWork.PromoCodes.UpdateAsync(promoCode);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePromoCodeAsync(Guid promoCodeId)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByIdAsync(promoCodeId);
        if (promoCode is null)
        {
            throw new NotFoundException("Promo code not found.");
        }

        await _unitOfWork.PromoCodes.DeleteAsync(promoCodeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(ValidatePromoCodeDto validateDto)
    {
        var promoCode = await _unitOfWork.PromoCodes.GetByCodeAsync(validateDto.Code);

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

        if (!promoCode.CanBeUsedByUser(validateDto.UserId))
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = "You have exceeded the maximum usage limit for this promo code."
            };

        if (promoCode.MinimumOrderAmount.HasValue && validateDto.OrderAmount < promoCode.MinimumOrderAmount.Value)
            return new PromoCodeValidationResultDto
            {
                IsValid = false,
                Message = $"Minimum order amount of {promoCode.MinimumOrderAmount:C} is required."
            };

        // Check if promo code is applicable to specific products
        if (!promoCode.ApplicableToAllProducts)
        {
            var hasApplicableProducts = validateDto.ProductIds.Any(pid =>
                promoCode.PromoCodeProducts.Any(p => p.ProductId == pid));

            var matchesCategory = validateDto.CategoryIds.Any(cid =>
                promoCode.PromoCodeCategories.Any(c => c.CategoryId == cid));

            if (!hasApplicableProducts && !matchesCategory)
                return new PromoCodeValidationResultDto
                {
                    IsValid = false,
                    Message = "Promo code is not applicable to the items in your cart."
                };
        }

        // Calculate discount amount
        var discountAmount = CalculateDiscount(promoCode, validateDto.OrderAmount);

        return new PromoCodeValidationResultDto
        {
            IsValid = true,
            Message = "Promo code is valid.",
            DiscountAmount = discountAmount,
            PromoCodeId = promoCode.PromoCodeId
        };
    }

    private static decimal CalculateDiscount(PromoCodeModel promoCode, decimal orderAmount)
    {
        decimal discount = promoCode.DiscountType == DiscountType.Percentage
            ? orderAmount * (promoCode.Value / 100)
            : promoCode.Value;

        if (promoCode.MaximumDiscountAmount.HasValue && discount > promoCode.MaximumDiscountAmount.Value)
            discount = promoCode.MaximumDiscountAmount.Value;

        return discount;
    }
}