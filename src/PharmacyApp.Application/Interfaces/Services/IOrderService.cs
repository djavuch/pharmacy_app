using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;
public interface IOrderService
{
    Task<PaginatedList<OrderSummaryDto>> GetAllOrdersAsync(QueryParams query);
    Task<Result<OrderDetailsDto>> GetOrderByIdAsync(int id, string userId, bool isStaff);
    Task<Result<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto createOrderDto, string userId, CancellationToken ct = default);
    Task<Result> CancelOrderAsync(int orderId, string userId, bool isStaff, CancellationToken ct = default);

    // Admin specific methods
    Task<Result> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto, CancellationToken ct = default);
    Task<Result> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto, CancellationToken ct = default);
}
