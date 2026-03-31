using FluentValidation;
using PharmacyApp.Application.DTOs.Address;
using PharmacyApp.Application.DTOValidations.Address;

public class SaveAddressValidator : AbstractValidator<SaveAddressDto>
{
    public SaveAddressValidator()
    {
        Include(new AddressValidator());

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(50);
    }
}