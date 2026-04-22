namespace PharmacyApp.Infrastructure.Options;

public class AdminBootstrapOptions
{
    public bool Enabled { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "System";
    public string LastName { get; set; } = "Admin";
}
