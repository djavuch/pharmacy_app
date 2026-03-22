using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Domain.Exceptions;

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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var (userId, isStaff) = GetCallerInfo();
        if (string.IsNullOrWhiteSpace(userId))
            throw new AppExceptions.UnauthorizedException("User is not authenticated.");

        var order = await _orderService.GetOrderByIdAsync(id, userId, isStaff);
        if (order is null) return NotFound();

        return Ok(order);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CreateOrderDto createOrderDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new AppExceptions.UnauthorizedException("Please sign in or create an account to place an order.");

        var order = await _orderService.CreateOrderAsync(createOrderDto, userId);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("cancel-order/{orderId}")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var (userId, isStaff) = GetCallerInfo();
        if (string.IsNullOrWhiteSpace(userId))
            throw new AppExceptions.UnauthorizedException("User is not authenticated.");

        await _orderService.CancelOrderAsync(orderId, userId, isStaff);
        return NoContent();
    }
}