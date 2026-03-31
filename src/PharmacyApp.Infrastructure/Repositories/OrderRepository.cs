using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.DTOs.Order;
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

    public IQueryable<OrderListDto> GetAllAsync()
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderListDto
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

    public async Task<OrderModel?> GetAllByUserIdAsync(int userId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == userId.ToString());
    }

    public async Task<OrderModel?> GetByIdAsync(int id)
    {
        return await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrderModel> AddAsync(OrderModel order)
    {
        await _dbContext.Orders.AddAsync(order);
        return order;
    }

    public Task UpdateAsync(OrderModel order)
    {
        _dbContext.Entry(order).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task UpdateOrderStatusAsync(int orderId, int statusId)
    {
        var order = await GetByIdAsync(orderId);
        if (order != null)
        {
            order.OrderStatus = (OrderStatus)statusId;
            await UpdateAsync(order);
        }
    }
}
