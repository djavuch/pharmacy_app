using PharmacyApp.Application.Interfaces.Email;

namespace PharmacyApp.IntegrationTests.Support;

internal sealed class NoOpOrderEmailNotifier : IOrderEmailNotifier
{
    public Task SendOrderConfirmationEmailAsync(int orderId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendOrderStatusUpdateEmailAsync(int orderId, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendOrderCancellationEmailAsync(int orderId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendOrderCompositionChangeEmailAsync(int orderId, CancellationToken ct = default)
        => Task.CompletedTask;
}
