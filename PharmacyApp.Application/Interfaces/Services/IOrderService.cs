using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Order;

namespace PharmacyApp.Application.Interfaces.Services;
public interface IOrderService
{
    Task<PaginatedList<OrderResponseDto>> GetAllOrdersAsync(int pageIndex = 1, int pageSize = 10, string? filterOn = null,
        string? filterQuery = null, string? sortBy = null, bool isAscending = true);
    Task<OrderResponseDto?> GetOrderByIdAsync(int id, string userId, bool isStaff);
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId);
    Task CancelOrderAsync(int orderId, string userId, bool isStaff);

    // Admin specific methods
    Task UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto);
    Task UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);
}
