using PharmacyApp.Application.DTOs.Email;

namespace PharmacyApp.Application.Interfaces.Email;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailRequestDto request);
}
