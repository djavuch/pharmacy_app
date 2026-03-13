using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Application.DTOs.User.AccountDto;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}
