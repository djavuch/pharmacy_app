using FluentValidation;
using PharmacyApp.Application.Contracts.User.Account;

namespace PharmacyApp.Application.DTOValidations.Users;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
