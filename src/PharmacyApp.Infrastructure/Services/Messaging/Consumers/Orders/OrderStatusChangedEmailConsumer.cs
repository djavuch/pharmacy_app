using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class OrderStatusChangedEmailConsumer : IConsumer<OrderStatusChangedMessage>
{
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly ILogger<OrderStatusChangedEmailConsumer> _logger;

    public OrderStatusChangedEmailConsumer(
        IOrderEmailNotifier orderEmailNotifier,
        ILogger<OrderStatusChangedEmailConsumer> logger)
    {
        _orderEmailNotifier = orderEmailNotifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Sending order status update email for order {OrderId}: {OldStatus} → {NewStatus}",
            message.OrderId, message.OldStatus, message.NewStatus);

        await _orderEmailNotifier.SendOrderStatusUpdateEmailAsync(
            message.OrderId, message.OldStatus, message.NewStatus, context.CancellationToken);
    }
}