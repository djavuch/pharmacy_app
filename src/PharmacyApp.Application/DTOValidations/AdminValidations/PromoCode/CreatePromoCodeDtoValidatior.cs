using FluentValidation;
using PharmacyApp.Application.Contracts.PromoCode;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.PromoCode;

public class CreatePromoCodeDtoValidator : AbstractValidator<CreatePromoCodeDto>
{
    public CreatePromoCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promo code is required.")
            .MinimumLength(3).WithMessage("Promo code must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Promo code cannot exceed 50 characters.")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Promo code can only contain uppercase letters, numbers, underscores and hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("Discount type is required.")
            .Must(x => x.Equals("Percentage", StringComparison.OrdinalIgnoreCase) ||
                      x.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Discount type must be 'Percentage' or 'FixedAmount'.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Percentage discount cannot exceed 100.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0)
            .When(x => x.MaxUsageCount.HasValue)
            .WithMessage("Max usage count must be greater than 0.");

        RuleFor(x => x.MaxUsagePerUser)
            .GreaterThan(0)
            .When(x => x.MaxUsagePerUser.HasValue)
            .WithMessage("Max usage per user must be greater than 0.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThan(0)
            .When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum order amount must be greater than 0.");

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaximumDiscountAmount.HasValue)
            .WithMessage("Maximum discount amount must be greater than 0.");

        RuleFor(x => x)
            .Must(x => x.ApplicableToAllProducts || x.ProductIds.Count > 0 || x.CategoryIds.Count > 0)
            .WithMessage("Promo code must be applicable to all products or specify at least one product/category.");
    }
}