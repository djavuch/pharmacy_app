using FluentValidation;
using PharmacyApp.Application.Contracts.Category.Admin;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.Categories;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.CategoryDescription)
            .MaximumLength(500).WithMessage("Category description cannot exceed 500 characters.");
    }
}