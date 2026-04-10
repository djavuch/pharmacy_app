using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IUserAddressRepository
{
    Task<List<UserAddress>> GetByUserIdAsync(string userId);
    Task<UserAddress?> GetByIdAsync(int id);
    Task SetDefaultAsync(int id, string userId);
    Task<UserAddress> AddAsync(UserAddress address);
    Task UpdateAsync(UserAddress address);
    Task DeleteAsync(int id);
}
