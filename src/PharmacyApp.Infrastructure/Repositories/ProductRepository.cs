using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class ProductRepository: IProductRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public ProductRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Product> GetAllAsync()
    {
        return _dbContext.Products
            .Include(p => p.Category)
            .AsQueryable();
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<Product> AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
        return product;
    }

    public Task UpdateAsync(Product product)
    {
        _dbContext.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int productId)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product != null)
        {
            _dbContext.Products.Remove(product);
        }
    }

    public async Task<List<Product>> GetByIdsAsync(List<int> productIds)
    {
        return await _dbContext.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Product> products)
    {
        _dbContext.Products.UpdateRange(products);
        await Task.CompletedTask;
    }
    
    public async Task UpdateWishlistCountAsync(int productId, int delta)
    {
        await _dbContext.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.WishlistCount, p => p.WishlistCount + delta));
    }
}
