using FluentValidation;
using PharmacyApp.Application.DTOs.Order;

namespace PharmacyApp.Application.DTOValidations.Orders;

public class OrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than zero.");
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}