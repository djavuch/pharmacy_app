namespace PharmacyApp.Application.Interfaces.Email;

public interface IAccountNotificationSender
{
    Task SendEmailForRegisterConfirmationAsync(
        string email, string userId, string token, string scheme, string host, CancellationToken ct);

    Task SendEmailForResetPasswordAsync(
        string email, string token, string scheme, string host, CancellationToken ct);
}
