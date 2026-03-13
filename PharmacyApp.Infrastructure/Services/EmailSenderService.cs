using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PharmacyApp.Application.DTOs.Email;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly EmailConfigurationDto _emailConfiguration;

    public EmailSenderService(IOptions<EmailConfigurationDto> emailConfiguration)
    {
        _emailConfiguration = emailConfiguration.Value;
    }

    public async Task SendEmailAsync(EmailRequestDto request)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.SmtpUser));
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.IsHtml ? request.Body : null,
            TextBody = request.IsHtml ? null : request.Body
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        await smtp.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.SslOnConnect);
        await smtp.AuthenticateAsync(_emailConfiguration.SmtpUser, _emailConfiguration.SmtpPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
