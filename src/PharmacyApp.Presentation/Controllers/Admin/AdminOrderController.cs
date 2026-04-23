using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[EnableCors("AllowFrontend")]
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

    [HttpGet]
    public async Task<ApiResponse> GetAllOrders([FromQuery] QueryParams query)
    {
        var orders = await _orderService.GetAllOrdersAsync(query);
        return new ApiResponse(true, null, orders);
    }

    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderDto updateOrderDto)
    {
        if (orderId != updateOrderDto.OrderId)
            return BadRequest(new { message = "Order id in URL does not match body." });
        
        var result = await _orderService.UpdateOrderAsync(orderId, updateOrderDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpPatch("{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderStatusDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();   
    }
}
