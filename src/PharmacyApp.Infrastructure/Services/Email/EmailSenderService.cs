using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyApp.Application.Contracts.Notifications.Email;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Infrastructure.Options;

namespace PharmacyApp.Infrastructure.Services.Email;

public class EmailSenderService : IEmailSenderService
{
    private readonly HttpClient _httpClient;
    private readonly ResendOptions _resendOptions;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(
        HttpClient httpClient,
        IOptions<ResendOptions> resendOptions,
        ILogger<EmailSenderService> logger)
    {
        _httpClient = httpClient;
        _resendOptions = resendOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequestDto request, CancellationToken ct)
    {
        ValidateResendOptions();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _resendOptions.ApiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resendOptions.ApiKey);
        httpRequest.Content = JsonContent.Create(BuildRequestBody(request));

        _logger.LogInformation("Sending email '{Subject}' to {Recipient} via Resend API.", request.Subject, request.To);

        using var response = await _httpClient.SendAsync(httpRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Resend API failed with status {(int)response.StatusCode} ({response.StatusCode}): {responseBody}");
        }

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.Accepted))
        {
            _logger.LogWarning("Resend API returned unexpected success status {StatusCode}.", response.StatusCode);
        }

        _logger.LogInformation("Email '{Subject}' to {Recipient} was accepted by Resend API.", request.Subject, request.To);
    }

    private object BuildRequestBody(EmailRequestDto request)
    {
        var from = string.IsNullOrWhiteSpace(_resendOptions.FromName)
            ? _resendOptions.FromEmail
            : $"{_resendOptions.FromName} <{_resendOptions.FromEmail}>";

        return request.IsHtml
            ? new
            {
                from,
                to = new[] { request.To },
                subject = request.Subject,
                html = request.Body
            }
            : new
            {
                from,
                to = new[] { request.To },
                subject = request.Subject,
                text = request.Body
            };
    }

    private void ValidateResendOptions()
    {
        if (string.IsNullOrWhiteSpace(_resendOptions.ApiKey))
        {
            throw new InvalidOperationException("Resend:ApiKey must be set.");
        }

        if (string.IsNullOrWhiteSpace(_resendOptions.FromEmail))
        {
            throw new InvalidOperationException("Resend:FromEmail must be set.");
        }

        if (string.IsNullOrWhiteSpace(_resendOptions.ApiUrl))
        {
            throw new InvalidOperationException("Resend:ApiUrl must be set.");
        }
    }
}

public record ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api.resend.com/emails";
}
