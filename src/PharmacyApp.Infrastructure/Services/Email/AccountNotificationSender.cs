using Microsoft.AspNetCore.WebUtilities;
using PharmacyApp.Domain.Entities;
using System.Text;
using PharmacyApp.Application.Contracts.Notifications.Email;
using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.Infrastructure.Services.Email;

public class AccountNotificationSender : IAccountNotificationSender
{
    private readonly IEmailSenderService _emailSenderService;

    public AccountNotificationSender(IEmailSenderService emailSenderService)
    {
        _emailSenderService = emailSenderService;
    }

    public async Task SendEmailForRegisterConfirmationAsync(
        string email, string userName, string token, string scheme, string host, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email is required for confirmation email.");

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var baseUrl = $"{scheme}://{host}".TrimEnd('/');
        var confirmationLink = QueryHelpers.AddQueryString(
            $"{baseUrl}/confirm-email",
            new Dictionary<string, string?>
            {
                ["userId"] = "", 
                ["token"] = encodedToken
            });
        var emailBody = $"<p>Hello {userName},<br/>Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a></p>";

        await _emailSenderService.SendEmailAsync(new EmailRequestDto
        {
            To = email,
            Subject = "Email Confirmation",
            Body = emailBody,
            IsHtml = true
        }, ct);
    }

    public async Task SendEmailForResetPasswordAsync(
        string email, string token, string scheme, string host, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email is required for password reset email.");

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var baseUrl = $"{scheme}://{host}".TrimEnd('/');
        var resetLink = QueryHelpers.AddQueryString(
            $"{baseUrl}/reset-password",
            new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = encodedToken
            });
        var emailBody = $"<p>Please reset your password by clicking this link: <a href='{resetLink}'>Reset Password</a></p>";

        await _emailSenderService.SendEmailAsync(new EmailRequestDto
        {
            To = email,
            Subject = "Password Reset",
            Body = emailBody,
            IsHtml = true
        }, ct);
    }
}
