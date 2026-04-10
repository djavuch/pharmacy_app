using FluentValidation;
using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.PromoCode;

public class ValidatePromoCodeDtoValidator : AbstractValidator<PromoCodeValidationResults>
{
    public ValidatePromoCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promo code is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID must be valid.");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Order amount must be greater than 0.");

        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("At least one product must be specified.");
    }
}