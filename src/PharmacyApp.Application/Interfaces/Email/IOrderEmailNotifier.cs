namespace PharmacyApp.Application.Interfaces.Email;

public interface IOrderEmailNotifier
{
    Task SendOrderConfirmationEmailAsync(int orderId);
    Task SendOrderStatusUpdateEmailAsync(int orderId, string oldStatus, string newStatus);
    Task SendOrderCancellationEmailAsync(int orderId);
    Task SendOrderCompositionChangeEmailAsync(int orderId);
}
