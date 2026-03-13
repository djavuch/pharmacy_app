using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createOrderValidator;

    public OrderController(IOrderService orderService,
        IValidator<CreateOrderDto> createOrderValidator)
    {
        _orderService = orderService;
        _createOrderValidator = createOrderValidator;    }

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

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id, userId!, isStaff);
            if (order is null) return NotFound();
            return Ok(order);
        }
        catch (UnauthorizedException)
        {
            return Forbid("You tried to access an order without proper authorization.");
        }
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CreateOrderDto createOrderDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return StatusCode(401, new { message = "Please sign in or create an account to place an order." });
        }

        var validationResult = await _createOrderValidator.ValidateAsync(createOrderDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var order = await _orderService.CreateOrderAsync(createOrderDto, userId);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("cancel-order/{orderId}")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var (userId, isStaff) = GetCallerInfo();

        try
        {
            await _orderService.CancelOrderAsync(orderId, userId!, isStaff);
            return NoContent();
        }
        catch (UnauthorizedException)
        {
            return Forbid("You tried to cancel an order without proper authorization.");
        }
    }
}