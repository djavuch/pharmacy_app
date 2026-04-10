using PharmacyApp.Application.Contracts.Address;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IUserAddressService
{
    Task<List<SavedAddressDto>> GetUserAddressesAsync(string userId);
    Task<SavedAddressDto?> GetAddressByIdAsync(int id, string userId);
    Task<SavedAddressDto> CreateAddressAsync(SaveAddressDto dto, string userId);
    Task<Result<SavedAddressDto>> UpdateAddressAsync(int id, SaveAddressDto dto, string userId);
    Task<Result> DeleteAddressAsync(int id, string userId);
    Task<Result> SetDefaultAddressAsync(int id, string userId);
}