namespace PharmacyApp.Application.DTOs.Email;

public record EmailConfigurationDto
{
    public string? FromName { get; set; }
    public string? SmtpServer { get; set; } 
    public int SmtpPort { get; set; }
    public string? SmtpUser { get; set; } 
    public string? SmtpPassword { get; set; }
}
