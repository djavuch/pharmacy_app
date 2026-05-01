using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PharmacyApp.Application.Contracts.Notifications.Email;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Infrastructure.Options;

namespace PharmacyApp.Infrastructure.Services.Email;

public class EmailSenderService : IEmailSenderService
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailSenderService> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequestDto request, CancellationToken ct)
    {
        ValidateEmailOptions();

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.SmtpUser));
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.IsHtml ? request.Body : null,
            TextBody = request.IsHtml ? null : request.Body
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        _logger.LogInformation(
            "Sending email '{Subject}' to {Recipient} via SMTP server {SmtpServer}:{SmtpPort}.",
            request.Subject,
            request.To,
            _emailOptions.SmtpServer,
            _emailOptions.SmtpPort);

        await smtp.ConnectAsync(_emailOptions.SmtpServer, _emailOptions.SmtpPort, SecureSocketOptions.StartTls, ct);
        await smtp.AuthenticateAsync(_emailOptions.SmtpUser, _emailOptions.SmtpPassword, ct);
        await smtp.SendAsync(email, ct);
        await smtp.DisconnectAsync(true, ct);

        _logger.LogInformation("Email '{Subject}' to {Recipient} was accepted by SMTP server.", request.Subject, request.To);
    }

    private void ValidateEmailOptions()
    {
        if (string.IsNullOrWhiteSpace(_emailOptions.SmtpServer))
        {
            throw new InvalidOperationException("EmailConfiguration:SmtpServer must be set.");
        }

        if (_emailOptions.SmtpPort <= 0)
        {
            throw new InvalidOperationException("EmailConfiguration:SmtpPort must be set to a valid port.");
        }

        if (string.IsNullOrWhiteSpace(_emailOptions.SmtpUser))
        {
            throw new InvalidOperationException("EmailConfiguration:SmtpUser must be set.");
        }

        if (string.IsNullOrWhiteSpace(_emailOptions.SmtpPassword))
        {
            throw new InvalidOperationException("EmailConfiguration:SmtpPassword must be set.");
        }
    }
}
