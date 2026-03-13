using PharmacyApp.Application.DTOs.Address;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IUserAddressService
{
    Task<List<SavedAddressDto>> GetUserAddressesAsync(string userId);
    Task<SavedAddressDto?> GetAddressByIdAsync(int id, string userId);
    Task<SavedAddressDto> CreateAddressAsync(SaveAddressDto dto, string userId);
    Task<SavedAddressDto> UpdateAddressAsync(int id, SaveAddressDto dto, string userId);
    Task DeleteAddressAsync(int id, string userId);
    Task SetDefaultAddressAsync(int id, string userId);
}