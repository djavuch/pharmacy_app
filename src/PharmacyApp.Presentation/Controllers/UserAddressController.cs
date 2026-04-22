using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Contracts.Address;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("user/addresses")]
[Authorize(Policy = "EmailConfirmed")]
public class AddressController : ControllerBase
{
    private readonly IUserAddressService _addressService;

    public AddressController(IUserAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var addresses = await _addressService.GetUserAddressesAsync(userId);
        return Ok(addresses);
    }

    [HttpGet("{id:int}")]
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

        var address = await _addressService.CreateAddressAsync(dto, userId);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, SaveAddressDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _addressService.UpdateAddressAsync(id, dto, userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var result = await _addressService.DeleteAddressAsync(id, userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpPatch("{id}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var result = await _addressService.SetDefaultAddressAsync(id, userId); 
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }
}