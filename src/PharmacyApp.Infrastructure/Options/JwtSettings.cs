namespace PharmacyApp.Infrastructure.Options;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;

    public string ResolveSecret()
        => string.IsNullOrWhiteSpace(SecretKey) ? Secret : SecretKey;
}
