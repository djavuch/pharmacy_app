using FluentValidation;
using PharmacyApp.Application.Contracts.Category.Admin;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.Categories;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");
        RuleFor(x => x.CategoryDescription)
            .MaximumLength(500).WithMessage("Category description cannot exceed 500 characters.");
    }
}
