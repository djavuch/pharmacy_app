using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class OrderCompositionChangedEmailConsumer : IConsumer<OrderCompositionChangedMessage>
{
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly ILogger<OrderCompositionChangedEmailConsumer> _logger;

    public OrderCompositionChangedEmailConsumer(
        IOrderEmailNotifier orderEmailNotifier,
        ILogger<OrderCompositionChangedEmailConsumer> logger)
    {
        _orderEmailNotifier = orderEmailNotifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompositionChangedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending order composition change email for order {OrderId}", message.OrderId);

        await _orderEmailNotifier.SendOrderCompositionChangeEmailAsync(message.OrderId, context.CancellationToken);
    }
}