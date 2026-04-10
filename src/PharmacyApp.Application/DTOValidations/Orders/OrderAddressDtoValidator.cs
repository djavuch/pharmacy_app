using FluentValidation;
using PharmacyApp.Application.Contracts.Order;

namespace PharmacyApp.Application.DTOValidations.Orders;

public class OrderAddressDtoValidator : AbstractValidator<OrderAddressDto>  
{
    public OrderAddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(100).WithMessage("Street cannot exceed 100 characters.");
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");
        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(50).WithMessage("State cannot exceed 50 characters.");
        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("ZipCode is required.")
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZipCode must be a valid format.");
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(50).WithMessage("Country cannot exceed 50 characters.");
        RuleFor(x => x.ApartmentNumber)
            .MaximumLength(5).WithMessage("Apartment Number cannot exceed 5 characters.");
    }
}
