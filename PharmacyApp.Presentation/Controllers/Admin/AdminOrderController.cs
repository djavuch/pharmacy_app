using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/orders")]
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public AdminOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("all-orders")]
    public async Task<ApiResponse> GetAllOrders(int pageIndex = 1, int pageSize = 10)
    {
        var orders = await _orderService.GetAllOrdersAsync(pageIndex, pageSize);
        return new ApiResponse(true, null, orders);
    }

    [HttpPut("update-order/{orderId}")]
    public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderDto updateOrderDto)

    {
        await _orderService.UpdateOrderAsync(orderId, updateOrderDto);
        return NoContent();
    }

    [HttpPatch("update-order-status/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, updateOrderStatusDto);
            return NoContent();

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }   
    }
}
