using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IOrderRepository
{
    IQueryable<OrderModel> GetAllAsync();
    Task<OrderModel?> GetAllByUserIdAsync(int userId);
    Task<OrderModel?> GetByIdAsync(int id);
    Task<OrderModel> AddAsync(OrderModel order);
    Task UpdateAsync(OrderModel order);
    Task UpdateOrderStatusAsync (int orderId, int statusId);
}
