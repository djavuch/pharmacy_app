namespace PharmacyApp.Infrastructure.Options;

public record EmailOptions
{
    public const string SectionName = "EmailConfiguration";
    
    public string FromName { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}
