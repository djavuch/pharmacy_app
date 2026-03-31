using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IProductRepository
{
    IQueryable<ProductModel> GetAllAsync();
    Task<ProductModel?> GetByIdAsync(int productId);
    Task<ProductModel> AddAsync(ProductModel product);
    Task UpdateAsync(ProductModel product);
    Task DeleteAsync(int productId);

    // Race condition safe stock update
    Task<List<ProductModel>> GetByIdsAsync(List<int> productIds);
    
    Task UpdateWishlistCountAsync(int productId, int delta);
}
