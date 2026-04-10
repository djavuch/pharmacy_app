using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public CategoryRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _dbContext.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(string categoryName)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryName == categoryName);
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(i => i.CategoryId == categoryId);
    }

    public async Task<Category> AddAsync(Category category)
    {
        await _dbContext.Categories.AddAsync(category);
        return category;
    }

    public Task UpdateAsync(Category category)
    {
        _dbContext.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int categoryId)
    {
        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category != null)
        {
            _dbContext.Categories.Remove(category);
        }
    }
}
