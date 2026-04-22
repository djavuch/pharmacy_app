using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Contracts.Admin;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWorkRepository _unitOfWork;

    public AdminDashboardService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var nowUtc = DateTime.UtcNow;

        var startOfDayUtc = nowUtc.Date;
        var startOfNextDayUtc = startOfDayUtc.AddDays(1);

        var startOfMonthUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfNextMonthUtc = startOfMonthUtc.AddMonths(1);

        var totalProducts = await _unitOfWork.Products.GetAllAsync().CountAsync();
        var ordersToday = await _unitOfWork.Orders.GetAllAsync()
            .Where(order => order.OrderDate >= startOfDayUtc && order.OrderDate < startOfNextDayUtc)
            .CountAsync();

        var monthlyRevenue = await _unitOfWork.Orders.GetAllAsync()
            .Where(order => order.OrderDate >= startOfMonthUtc && order.OrderDate < startOfNextMonthUtc)
            .Where(order => order.OrderStatus != OrderStatus.Cancelled)
            .SumAsync(order => (decimal?)order.TotalAmount);

        return new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            OrdersToday = ordersToday,
            MonthlyRevenue = monthlyRevenue ?? 0m
        };
    }
}
