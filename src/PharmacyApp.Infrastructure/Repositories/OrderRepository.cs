using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public OrderRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<OrderSummaryDto> GetAllAsync()
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                BuyerFirstName = o.User.FirstName ?? string.Empty,
                BuyerLastName = o.User.LastName ?? string.Empty,
                ItemsCount = o.OrderItems.Count()
            });
    }

    public async Task<Order?> GetAllByUserIdAsync(int userId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == userId.ToString());
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
        return order;
    }

    public Task UpdateAsync(Order order)
    {
        _dbContext.Entry(order).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}
