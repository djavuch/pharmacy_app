using FluentValidation;
using PharmacyApp.Application.DTOs.Discount;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.Discounts;

public class UpdateDiscountDtoValidator : AbstractValidator<UpdateDiscountDto>
{
    public UpdateDiscountDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Discount name is required.")
            .MaximumLength(200).WithMessage("Discount name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("Discount type is required.")
            .Must(t => Enum.TryParse<DiscountType>(t, ignoreCase: true, out _))
            .WithMessage($"Invalid discount type. Valid values: {string.Join(", ", Enum.GetNames<DiscountType>())}.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

        RuleFor(x => x)
            .Must(x => !x.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase) || x.Value <= 100)
            .WithMessage("Percentage discount cannot exceed 100%.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThan(0).When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum order amount must be greater than 0.");

        RuleFor(x => x.MaximumOrderAmount)
            .GreaterThan(x => x.MinimumOrderAmount ?? 0)
            .When(x => x.MaximumOrderAmount.HasValue)
            .WithMessage("Maximum order amount must be greater than minimum order amount.");
    }
}