namespace PharmacyApp.Infrastructure.Options;

public record FrontendOptions
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; set; } = string.Empty;
}
