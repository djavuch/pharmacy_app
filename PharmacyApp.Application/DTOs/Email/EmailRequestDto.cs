namespace PharmacyApp.Application.DTOs.Email;

public record EmailRequestDto
{
    public string To { get; set; }  = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}
