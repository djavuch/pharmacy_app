namespace PharmacyApp.Application.DTOs.Bonus;

public record BonusAccountDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserFullName { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}