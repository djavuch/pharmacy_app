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

    public async Task<List<UserAddressModel>> GetByUserIdAsync(string userId)
    {
        return await _dbContext.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserAddressModel?> GetByIdAsync(int id)
    {
        return await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<UserAddressModel?> GetDefaultByUserIdAsync(string userId)
    {
        return await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
    }

    public async Task<UserAddressModel> AddAsync(UserAddressModel address)
    {
        if (address.IsDefault)
        {
            var existingDefaults = await _dbContext.UserAddresses
                .Where(a => a.UserId == address.UserId && a.IsDefault)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        await _dbContext.UserAddresses.AddAsync(address);
        return address;
    }

    public Task UpdateAsync(UserAddressModel address)
    {
        address.UpdatedAt = DateTime.UtcNow;
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