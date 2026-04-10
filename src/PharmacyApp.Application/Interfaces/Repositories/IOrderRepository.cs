using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IOrderRepository
{
    IQueryable<OrderSummaryDto> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
}
