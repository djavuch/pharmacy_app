using FluentValidation;
using PharmacyApp.Application.Contracts.Address;

namespace PharmacyApp.Application.DTOValidations.Address;

public class AddressValidator : AbstractValidator<AddressDetailsDto>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200);

        RuleFor(x => x.ApartmentNumber)
            .MaximumLength(50);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100);

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100);

        RuleFor(x => x.AdditionalInfo)
            .MaximumLength(500);
    }
}