using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IProductRepository
{
    IQueryable<Product> GetAllAsync();
    Task<Product?> GetByIdAsync(int productId);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int productId);

    // Race condition safe stock update
    Task<List<Product>> GetByIdsAsync(List<int> productIds);
    
    Task UpdateWishlistCountAsync(int productId, int delta);
}
