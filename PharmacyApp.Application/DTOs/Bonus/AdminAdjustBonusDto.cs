using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Application.DTOs.Bonus;

public class AdminAdjustBonusDto
{
    [Required]
    public decimal Points { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}