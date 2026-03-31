using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IOrderRepository
{
    IQueryable<OrderListDto> GetAllAsync();
    Task<OrderModel?> GetByIdAsync(int id);
    Task<OrderModel> AddAsync(OrderModel order);
    Task UpdateAsync(OrderModel order);
}
