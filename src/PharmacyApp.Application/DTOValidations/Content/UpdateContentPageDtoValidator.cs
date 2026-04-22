using FluentValidation;
using PharmacyApp.Application.Contracts.Content.Admin;

namespace PharmacyApp.Application.DTOValidations.Content;

public class UpdateContentPageDtoValidator : AbstractValidator<UpdateContentPageDto>
{
    public UpdateContentPageDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot be longer than 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(50000).WithMessage("Content cannot be longer than 50000 characters.");
    }
}
