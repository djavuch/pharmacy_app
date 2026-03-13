using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Address;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("user/addresses")]
[Authorize(Policy = "EmailConfirmed")]
public class AddressController : ControllerBase
{
    private readonly IUserAddressService _addressService;
    private readonly IValidator<SaveAddressDto> _saveValidator;

    public AddressController(IUserAddressService addressService, IValidator<SaveAddressDto> saveValidator)
    {
        _addressService = addressService;
        _saveValidator = saveValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var addresses = await _addressService.GetUserAddressesAsync(userId);
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var address = await _addressService.GetAddressByIdAsync(id, userId);

        return address is null ? NotFound() : Ok(address);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress(SaveAddressDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var validationResult = await _saveValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var address = await _addressService.CreateAddressAsync(dto, userId);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, SaveAddressDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var validationResult = await _saveValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var address = await _addressService.UpdateAddressAsync(id, dto, userId);
            return Ok(address);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            await _addressService.DeleteAddressAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            await _addressService.SetDefaultAddressAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}