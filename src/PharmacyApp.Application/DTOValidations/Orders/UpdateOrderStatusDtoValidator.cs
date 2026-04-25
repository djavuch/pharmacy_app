using FluentValidation;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.DTOValidations.Orders;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status.");
    }
}
