using PharmacyApp.Application.Contracts.Notifications.Email;

namespace PharmacyApp.Application.Interfaces.Email;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailRequestDto request, CancellationToken ct = default);
}
