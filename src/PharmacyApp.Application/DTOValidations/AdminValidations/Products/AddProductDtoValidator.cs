using FluentValidation;
using PharmacyApp.Application.DTOs.Product;

namespace PharmacyApp.Application.DTOValidations.AdminValidations.Products;

public class AddProductDtoValidator : AbstractValidator<CreateProductDto>

{
    public AddProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Product description cannot exceed 500 characters.");
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID is required.");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}
