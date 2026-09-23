namespace PharmacyApp.Application.Interfaces.Email;

public interface IOrderEmailNotifier
{
    Task SendOrderConfirmationEmailAsync(int orderId, CancellationToken ct = default);
    Task SendOrderStatusUpdateEmailAsync(int orderId, string oldStatus, string newStatus, CancellationToken ct = default);
    Task SendOrderCancellationEmailAsync(int orderId, CancellationToken ct = default);
    Task SendOrderCompositionChangeEmailAsync(int orderId, CancellationToken ct = default);
}