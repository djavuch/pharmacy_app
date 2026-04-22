using FluentValidation;
using PharmacyApp.Application.Contracts.User.Profile;

namespace PharmacyApp.Application.DTOValidations.Users;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
    }
}
