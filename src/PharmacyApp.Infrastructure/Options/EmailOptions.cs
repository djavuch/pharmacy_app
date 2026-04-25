namespace PharmacyApp.Infrastructure.Options;

public record EmailOptions
{
    public const string SectionName = "EmailConfiguration";
    
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseStartTls { get; set; } = true;
    public bool UseAuthentication { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}
