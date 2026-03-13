namespace PharmacyApp.Application.DTOs.User.Enums;

public enum LoginFailureReason
{
    InvalidCredentials,
    EmailNotConfirmed,
    PasswordResetRequired,
    AccountLocked
}