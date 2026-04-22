using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IProductRepository
{
    IQueryable<Product> GetAllAsync();
    Task<Product?> GetByIdAsync(int productId);
    Task<Product?> GetByIdWithCategoryAsync(int productId);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int productId);

    Task<List<Product>> GetByIdsAsync(List<int> productIds);
    Task<int> TryAdjustStockAsync(int productId, int quantityChange);

    Task UpdateWishlistCountAsync(int productId, int delta);
}
