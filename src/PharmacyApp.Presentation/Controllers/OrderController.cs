using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Contracts.Order;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService,
        IValidator<CreateOrderDto> createOrderValidator)
    {
        _orderService = orderService;
    }

    private (string? userId, bool isStaff) GetCallerInfo() =>
    (
        User.FindFirstValue(ClaimTypes.NameIdentifier),
        User.IsInRole("Admin") || User.IsInRole("Pharmacist") || User.IsInRole("Manager")
    );

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var (userId, isStaff) = GetCallerInfo();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _orderService.GetOrderByIdAsync(id, userId, isStaff);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return Ok(result.Value);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CreateOrderDto createOrderDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _orderService.CreateOrderAsync(createOrderDto, userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return CreatedAtAction(nameof(GetOrder), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("/orders/{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var (userId, isStaff) = GetCallerInfo();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _orderService.CancelOrderAsync(orderId, userId, isStaff);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return NoContent();
    }
}