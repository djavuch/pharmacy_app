namespace PharmacyApp.Application.Contracts.Messages;

public record EmailMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
}

public record SendRegistrationEmailMessage : EmailMessage
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string ConfirmationToken { get; init; }
    public required string Scheme { get; init; }
    public required string Host { get; init; }
}

public record SendPasswordResetEmailMessage : EmailMessage
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string ResetToken { get; init; }
    public required string Scheme { get; init; }
    public required string Host { get; init; }
}

public record OrderCreatedMessage : EmailMessage
{
    public required int OrderId { get; init; }
    public required string UserEmail { get; init; }
    public required string UserName { get; init; }
}

public record OrderStatusChangedMessage : EmailMessage
{
    public required int OrderId { get; init; }
    public required string UserEmail { get; init; }
    public required string UserName { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
}

public record OrderCancelledMessage : EmailMessage
{
    public required int OrderId { get; init; }
    public required string UserEmail { get; init; }
    public required string UserName { get; init; }
}

public record OrderCompositionChangedMessage : EmailMessage
{
    public required int OrderId { get; init; }
    public required string UserEmail { get; init; }
    public required string UserName { get; init; }
}