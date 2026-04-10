using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    
    public UserAddressRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserAddress>> GetByUserIdAsync(string userId)
    {
        return await _dbContext.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserAddress?> GetByIdAsync(int id)
    {
        return await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task SetDefaultAsync(int addressId, string userId)
    {
        await _dbContext.UserAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
        
        await _dbContext.UserAddresses
            .Where(a => a.Id == addressId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, true));
    }

    public async Task<UserAddress> AddAsync(UserAddress address)
    {
        if (address.IsDefault)
        {
            var existingDefaults = await _dbContext.UserAddresses
                .Where(a => a.UserId == address.UserId && a.IsDefault)
                .ToListAsync();

            foreach (var existing in existingDefaults)
                existing.UnsetDefault();
        }

        await _dbContext.UserAddresses.AddAsync(address);
        return address;
    }

    public Task UpdateAsync(UserAddress address)
    {
        address.MarkUpdated();
        _dbContext.Entry(address).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(int id)
    {
        var address = await GetByIdAsync(id);
        if (address != null)
        {
            _dbContext.UserAddresses.Remove(address);
        }
    }
}