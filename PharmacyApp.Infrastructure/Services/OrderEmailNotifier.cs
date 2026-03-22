using PharmacyApp.Application.DTOs.Email;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Domain.Entities;
using System.Text;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Infrastructure.Services;

public class OrderEmailNotifier : IOrderEmailNotifier
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly IEmailSenderService _emailSenderService;

    public OrderEmailNotifier(IUnitOfWorkRepository unitOfWork, IEmailSenderService emailSenderService)
    {
        _unitOfWork = unitOfWork;
        _emailSenderService = emailSenderService;
    }

    public async Task SendOrderConfirmationEmailAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order is null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        var subject = $"Order Confirmation - Order #{order.Id}";
        var body = BuildOrderConfirmationEmailBody(order);

        var emailRequest = new EmailRequestDto
        {
            To = order.User.Email,
            Subject = subject,
            Body = body
        };

        await _emailSenderService.SendEmailAsync(emailRequest);
    }

    public async Task SendOrderStatusUpdateEmailAsync(int orderId, string oldStatus, string newStatus)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order is null || order.User is null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found.");
        }

        var subject = $"Order Status Update - Order #{order.Id}";
        var body = BuildOrderStatusUpdateEmailBody(order, newStatus);

        var emailRequest = new EmailRequestDto
        {
            To = order.User.Email,
            Subject = subject,
            Body = body
        };

        await _emailSenderService.SendEmailAsync(emailRequest);
    }

    public async Task SendOrderCancellationEmailAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order is null || order.User is null)
        {
            throw new NotFoundException("Order or associated user not found.");
        }

        var subject = $"Order Cancellation - Order #{order.Id}";
        var body = BuildOrderCancellationEmailBody(order);

        var emailRequest = new EmailRequestDto
        {
            To = order.User.Email,
            Subject = subject,
            Body = body
        };

        await _emailSenderService.SendEmailAsync(emailRequest);
    }

    private string BuildOrderConfirmationEmailBody(OrderModel order)
    {
        var sb = new StringBuilder();
        sb.Append("<html><body>");
        sb.AppendLine($"<h2>Dear {order.User.FirstName + " " + order.User.LastName}!</h2>");
        sb.AppendLine($"<p>Thank you for your order #{order.Id}.</p>");
        sb.AppendLine($"<p>Your order details are as follows:</p>");
        sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'>");
        sb.AppendLine("<tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total</th></tr>");
        foreach (var item in order.OrderItems)
        {
            sb.AppendLine($"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>{item.Price:C}</td><td>{item.Quantity * item.Price:C}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<tr><td colspan='3'>Total Price</td><td>{order.TotalAmount:C}</td></tr>");

        if (order.ShippingAddress != null)
        {
            sb.AppendLine("<h3>Shipping address:</h3>");
            sb.AppendLine($"<p>{order.ShippingAddress.Street}, apt. {order.ShippingAddress.ApartmentNumber}<br/>");
            sb.AppendLine($"{order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.ZipCode}<br/>");
            sb.AppendLine($"{order.ShippingAddress.Country}</p>");
        }

        sb.AppendLine("<p>Sincerely,<br/>PharmacyApp Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string BuildOrderStatusUpdateEmailBody(OrderModel order, string newStatus)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine($"<h2>Dear {order.User.FirstName + " " + order.User.LastName}!</h2>");
        sb.AppendLine($"<p>Your order #{order.Id} status has been updated.");
        sb.AppendLine($"<p><strong>New status:</strong> {newStatus}</p>");
        sb.AppendLine($"<p><strong>Order date:</strong> {order.OrderDate:dd.MM.yyyy HH:mm}</p>");
        sb.AppendLine($"<p><strong>Total:</strong> {order.TotalAmount:C}</p>");
        sb.AppendLine("<p>Sincerely,<br/>PharmacyApp Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string BuildOrderCancellationEmailBody(OrderModel order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
        sb.AppendLine($"<h2>Dead, {order.User.FirstName} {order.User.LastName}!</h2>");
        sb.AppendLine($"<p>Your order #{order.Id} status has been updated.");
        sb.AppendLine("<div style='background-color: #ffebee; padding: 15px; border-radius: 5px; border-left: 4px solid #f44336;'>");
        sb.AppendLine($"<p><strong>Order:</strong> {order.Id}</p>");
        sb.AppendLine($"<p><strong>Order date:</strong> {order.OrderDate:dd.MM.yyyy HH:mm}</p>");
        sb.AppendLine($"<p><strong>Total:</strong> {order.TotalAmount:C}</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<p>If you have not canceled this order, please contact our customer service department.</p>");
        sb.AppendLine("<p>Sincerely,<br/>PharmacyApp Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
