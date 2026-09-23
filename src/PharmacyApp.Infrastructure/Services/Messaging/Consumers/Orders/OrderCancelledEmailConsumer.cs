using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class OrderCancelledEmailConsumer : IConsumer<OrderCancelledMessage>
{
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly ILogger<OrderCancelledEmailConsumer> _logger;

    public OrderCancelledEmailConsumer(
        IOrderEmailNotifier orderEmailNotifier,
        ILogger<OrderCancelledEmailConsumer> logger)
    {
        _orderEmailNotifier = orderEmailNotifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending order cancellation email for order {OrderId}", message.OrderId);

        await _orderEmailNotifier.SendOrderCancellationEmailAsync(message.OrderId, context.CancellationToken);
    }
}