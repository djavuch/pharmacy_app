using PharmacyApp.Application.DTOs.Email;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Email;

public interface IAccountNotificationSender
{
    Task SendEmailForRegisterConfirmationAsync(UserModel user, string token, string scheme, string host, CancellationToken ct);
    Task SendEmailForResetPasswordAsync(UserModel user, string token, string scheme, string host, CancellationToken ct);
}
