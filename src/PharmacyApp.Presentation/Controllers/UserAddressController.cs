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

        var address = await _addressService.CreateAddressAsync(dto, userId);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, SaveAddressDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var address = await _addressService.UpdateAddressAsync(id, dto, userId);
        return Ok(address);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _addressService.DeleteAddressAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("{id}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _addressService.SetDefaultAddressAsync(id, userId);
        return NoContent();
    }
}