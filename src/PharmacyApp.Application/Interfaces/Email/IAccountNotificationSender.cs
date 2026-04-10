using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Email;

public interface IAccountNotificationSender
{
    Task SendEmailForRegisterConfirmationAsync(User user, string token, string scheme, string host, CancellationToken ct);
    Task SendEmailForResetPasswordAsync(User user, string token, string scheme, string host, CancellationToken ct);
}
