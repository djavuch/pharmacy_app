namespace PharmacyApp.Application.Contracts.User.Results;

public enum LoginFailureReason
{
    InvalidCredentials,
    EmailNotConfirmed,
    PasswordResetRequired,
    AccountLocked
}