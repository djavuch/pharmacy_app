using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class OrderCreatedEmailConsumer : IConsumer<OrderCreatedMessage>
{
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly ILogger<OrderCreatedEmailConsumer> _logger;

    public OrderCreatedEmailConsumer(
        IOrderEmailNotifier orderEmailNotifier,
        ILogger<OrderCreatedEmailConsumer> logger)
    {
        _orderEmailNotifier = orderEmailNotifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending order confirmation email for order {OrderId}", message.OrderId);

        await _orderEmailNotifier.SendOrderConfirmationEmailAsync(message.OrderId, context.CancellationToken);
    }
}