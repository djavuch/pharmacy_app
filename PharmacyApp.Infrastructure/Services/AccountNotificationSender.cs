using Microsoft.AspNetCore.WebUtilities;
using PharmacyApp.Application.DTOs.Email;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Domain.Entities;
using System.Text;

namespace PharmacyApp.Infrastructure.Services;

public class AccountNotificationSender : IAccountNotificationSender
{
    private readonly IEmailSenderService _emailSenderService;

    public AccountNotificationSender(IEmailSenderService emailSenderService)
    {
        _emailSenderService = emailSenderService;
    }

    public async Task SendEmailForRegisterConfirmationAsync(UserModel user, string token, string scheme, string host)
    {
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationLink = $"{scheme}://{host}/account/confirm-email?userId={user.Id}&token={encodedToken}";
        var emailBody = $"<p>Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a></p>";

        await _emailSenderService.SendEmailAsync(new EmailRequestDto
        {
            To = user.Email,
            Subject = "Email Confirmation",
            Body = emailBody,
            IsHtml = true
        });
    }

    public async Task SendEmailForResetPasswordAsync(UserModel user, string token, string scheme, string host)
    {
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = $"{scheme}://{host}/reset-password-page?email={user.Email}&token={encodedToken}"; 
        var emailBody = $"<p>Please reset your password by clicking this link: <a href='{resetLink}'>Reset Password</a></p>";

        await _emailSenderService.SendEmailAsync(new EmailRequestDto
        {
            To = user.Email,
            Subject = "Password Reset",
            Body = emailBody,
            IsHtml = true
        });
    }
}
