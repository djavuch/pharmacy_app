using FluentValidation;
using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.DTOValidations.Address;

namespace PharmacyApp.Application.DTOValidations.Orders;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.SavedAddressId.HasValue || x.NewAddress != null)
            .WithMessage("You must provide either SavedAddressId or NewAddress");

        RuleFor(x => x)
            .Must(x => !x.SavedAddressId.HasValue || x.NewAddress == null)
            .WithMessage("Cannot provide both SavedAddressId and NewAddress. Choose one");

        When(x => x.NewAddress != null, () =>
        {
            RuleFor(x => x.NewAddress!)
                .SetValidator(new AddressValidator());
        });

        When(x => x.SaveAddress, () =>
        {
            RuleFor(x => x.SavedLabel)
                .NotEmpty().WithMessage("Address label is required when saving address to profile")
                .MaximumLength(50);
        });
    }
}
