using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Domain.Entities;
using DomainOrder = PharmacyApp.Domain.Entities.Order;

namespace PharmacyApp.Application.Contracts.Messages;

internal static class OrderMessages
{
    public static OrderCreatedMessage Created(DomainOrder order) => new()
    {
        To = order.User!.Email,
        Subject = $"Order Confirmation - Order #{order.Id}",
        Body = string.Empty,
        OrderId = order.Id,
        UserEmail = order.User.Email,
        UserName = $"{order.User.FirstName} {order.User.LastName}"
    };

    public static OrderStatusChangedMessage StatusChanged(DomainOrder order, string oldStatus, string newStatus) => new()
    {
        To = order.User!.Email,
        Subject = $"Order Status Update - Order #{order.Id}",
        Body = string.Empty,
        OrderId = order.Id,
        UserEmail = order.User.Email,
        UserName = $"{order.User.FirstName} {order.User.LastName}",
        OldStatus = oldStatus,
        NewStatus = newStatus
    };

    public static OrderCancelledMessage Cancelled(DomainOrder order) => new()
    {
        To = order.User!.Email,
        Subject = $"Order Cancellation - Order #{order.Id}",
        Body = string.Empty,
        OrderId = order.Id,
        UserEmail = order.User.Email,
        UserName = $"{order.User.FirstName} {order.User.LastName}"
    };

    public static OrderCompositionChangedMessage CompositionChanged(DomainOrder order) => new()
    {
        To = order.User!.Email,
        Subject = $"Order Composition Changed - Order #{order.Id}",
        Body = string.Empty,
        OrderId = order.Id,
        UserEmail = order.User.Email,
        UserName = $"{order.User.FirstName} {order.User.LastName}"
    };
}