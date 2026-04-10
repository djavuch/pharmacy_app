using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IDiscountRepository
{
    Task<Discount?> GetByIdAsync(Guid discountId);
    Task<IEnumerable<Discount>> GetAllAsync();
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
    Task<Discount> AddAsync(Discount discount);
    Task UpdateAsync(Discount discount);
    Task DeleteAsync(Guid discountId);

    // Auto-implemented methods for fetching discounts by product and category
    Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId);
    Task<IEnumerable<Discount>> GetDiscountsByCategoryIdAsync(int categoryId);
}
