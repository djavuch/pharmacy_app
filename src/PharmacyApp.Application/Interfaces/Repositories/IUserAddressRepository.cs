using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IUserAddressRepository
{
    Task<List<UserAddressModel>> GetByUserIdAsync(string userId);
    Task<UserAddressModel?> GetByIdAsync(int id);
    Task<UserAddressModel?> GetDefaultByUserIdAsync(string userId);
    Task<UserAddressModel> AddAsync(UserAddressModel address);
    Task UpdateAsync(UserAddressModel address);
    Task DeleteAsync(int id);
}
