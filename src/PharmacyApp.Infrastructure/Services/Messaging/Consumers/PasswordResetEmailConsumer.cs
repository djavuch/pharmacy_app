using MassTransit;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.Messages;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Messaging.Consumers;

public class PasswordResetEmailConsumer : IConsumer<SendPasswordResetEmailMessage>
{
    private readonly IAccountNotificationSender _notificationSender;
    private readonly ILogger<PasswordResetEmailConsumer> _logger;

    public PasswordResetEmailConsumer(
        IAccountNotificationSender notificationSender,
        ILogger<PasswordResetEmailConsumer> logger)
    {
        _notificationSender = notificationSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendPasswordResetEmailMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing password reset email for user {UserId}", message.UserId);

        await _notificationSender.SendEmailForResetPasswordAsync(
            message.To,
            message.ResetToken,
            message.Scheme,
            message.Host,
            context.CancellationToken);
    }
}