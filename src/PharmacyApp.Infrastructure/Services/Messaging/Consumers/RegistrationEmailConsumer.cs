using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class RegistrationEmailConsumer : IConsumer<SendRegistrationEmailMessage>
{
    private readonly IAccountNotificationSender _notificationSender;
    private readonly ILogger<RegistrationEmailConsumer> _logger;

    public RegistrationEmailConsumer(
        IAccountNotificationSender notificationSender,
        ILogger<RegistrationEmailConsumer> logger)
    {
        _notificationSender = notificationSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendRegistrationEmailMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing registration email for user {UserId}", message.UserId);
        
        await _notificationSender.SendEmailForRegisterConfirmationAsync(
            message.UserId,
            message.UserName,
            message.ConfirmationToken,
            message.Scheme,
            message.Host,
            context.CancellationToken);
    }
}