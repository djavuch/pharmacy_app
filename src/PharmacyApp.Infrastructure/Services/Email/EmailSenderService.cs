using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PharmacyApp.Application.Contracts.Notifications.Email;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Infrastructure.Options;

namespace PharmacyApp.Infrastructure.Services.Email;

public class EmailSenderService : IEmailSenderService
{
    private readonly EmailOptions _emailOptions;

    public EmailSenderService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendEmailAsync(EmailRequestDto request, CancellationToken ct)
    {
        var fromEmail = string.IsNullOrWhiteSpace(_emailOptions.FromEmail)
            ? _emailOptions.SmtpUser
            : _emailOptions.FromEmail;

        fromEmail = fromEmail.Trim();

        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "EmailConfiguration:FromEmail must be set when SMTP authentication is disabled or SmtpUser is empty.");
        }

        if (!MailboxAddress.TryParse(fromEmail, out var parsedFromAddress))
        {
            throw new InvalidOperationException(
                $"EmailConfiguration:FromEmail contains an invalid email address: '{fromEmail}'.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailOptions.FromName, parsedFromAddress.Address));
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.IsHtml ? request.Body : null,
            TextBody = request.IsHtml ? null : request.Body
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        var socketOptions = _emailOptions.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await smtp.ConnectAsync(_emailOptions.SmtpServer, _emailOptions.SmtpPort, socketOptions, ct);

        if (_emailOptions.UseAuthentication && !string.IsNullOrWhiteSpace(_emailOptions.SmtpUser))
        {
            await smtp.AuthenticateAsync(_emailOptions.SmtpUser, _emailOptions.SmtpPassword, ct);
        }

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true, ct);
    }
}
