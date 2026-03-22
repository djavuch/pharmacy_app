using PharmacyApp.Application.DTOs.Discount;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Enums;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWorkRepository _unitOfWork;

    public DiscountService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto createDiscountDto)
    {
        if (!Enum.TryParse<DiscountType>(createDiscountDto.DiscountType, ignoreCase: true, out var discountType))
            throw new BadRequestException(
                $"Invalid discount type: '{createDiscountDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");

        var discount = new DiscountModel
        {
            DiscountId = Guid.NewGuid(),
            Name = createDiscountDto.Name,
            Description = createDiscountDto.Description,
            DiscountType = Enum.Parse<DiscountType>(createDiscountDto.DiscountType),
            Value = createDiscountDto.Value,
            StartDate = DateTime.SpecifyKind(createDiscountDto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(createDiscountDto.EndDate, DateTimeKind.Utc),
            IsActive = createDiscountDto.IsActive,
            MinimumOrderAmount = createDiscountDto.MinimumOrderAmount,
            MaximumOrderAmount = createDiscountDto.MaximumOrderAmount
        };

        discount.ValidateBusinessRules();

        discount.ProductDiscounts = createDiscountDto.ProductIds
            .Select(pid => new ProductDiscountModel { ProductId = pid, DiscountId = discount.DiscountId })
            .ToList();

        discount.CategoryDiscounts = createDiscountDto.CategoryIds
            .Select(cid => new CategoryDiscountModel { CategoryId = cid, DiscountId = discount.DiscountId })
            .ToList();

        var createdDiscount = await _unitOfWork.Discounts.AddAsync(discount);
        return createdDiscount.ToDiscountDto();
    }

    public async Task<DiscountDto?> GetDiscountByIdAsync(Guid discountId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);
        return discount?.ToDiscountDto();
    }

    public async Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetAllAsync();
        return discounts.Select(d => d.ToDiscountDto());
    }

    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetActiveDiscountsAsync();
        return discounts.Select(d => d.ToDiscountDto());
    }

    public async Task UpdateDiscountAsync(Guid discountId, UpdateDiscountDto updateDiscountDto)
    {
        if (!Enum.TryParse<DiscountType>(updateDiscountDto.DiscountType, ignoreCase: true, out var discountType))
            throw new BadRequestException(
                $"Invalid discount type: '{updateDiscountDto.DiscountType}'. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");

        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);

        if (discount is null)
        {
            throw new NotFoundException("Discount not found");
        }

        discount.Name = updateDiscountDto.Name;
        discount.Description = updateDiscountDto.Description;
        discount.DiscountType = Enum.Parse<DiscountType>(updateDiscountDto.DiscountType);
        discount.Value = updateDiscountDto.Value;
        discount.StartDate = DateTime.SpecifyKind(updateDiscountDto.StartDate, DateTimeKind.Utc);
        discount.EndDate = DateTime.SpecifyKind(updateDiscountDto.EndDate, DateTimeKind.Utc);
        discount.IsActive = updateDiscountDto.IsActive;
        discount.MinimumOrderAmount = updateDiscountDto.MinimumOrderAmount;
        discount.MaximumOrderAmount = updateDiscountDto.MaximumOrderAmount;

        discount.ValidateBusinessRules();

        discount.ProductDiscounts.Clear();
        foreach (var pid in updateDiscountDto.ProductIds)
        {
            discount.ProductDiscounts.Add(new ProductDiscountModel { ProductId = pid, DiscountId = discountId });
        }

        discount.CategoryDiscounts.Clear();
        foreach (int cid in updateDiscountDto.CategoryIds)
        {
            discount.CategoryDiscounts.Add(new CategoryDiscountModel { CategoryId = cid, DiscountId = discountId });
        }

        await _unitOfWork.Discounts.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteDiscountAsync(Guid discountId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);

        if (discount is null)
        {
            throw new NotFoundException($"Discount {discountId} not found.");
        }

        await _unitOfWork.Discounts.DeleteAsync(discountId);
    }

    public async Task<decimal> CalculateDiscountedPriceAsync(int productId, int categoryId, decimal originalPrice)
    {
        var productDiscounts = await _unitOfWork.Discounts.GetDiscountsByProductIdAsync(productId);
        var categoryDiscounts = await _unitOfWork.Discounts.GetDiscountsByCategoryIdAsync(categoryId);

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

    private decimal CalculateAmount(DiscountModel discount, decimal price)
    {
        var amount = discount.DiscountType == DiscountType.Percentage
            ? price * discount.Value / 100
            : discount.Value;

        return Math.Round(amount, 2);
    }
}
