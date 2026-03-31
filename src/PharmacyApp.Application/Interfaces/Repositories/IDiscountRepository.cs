using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IDiscountRepository
{
    Task<DiscountModel?> GetByIdAsync(Guid discountId);
    Task<IEnumerable<DiscountModel>> GetAllAsync();
    Task<IEnumerable<DiscountModel>> GetActiveDiscountsAsync();
    Task<DiscountModel> AddAsync(DiscountModel discount);
    Task UpdateAsync(DiscountModel discount);
    Task DeleteAsync(Guid discountId);

    // Auto-implemented methods for fetching discounts by product and category
    Task<IEnumerable<DiscountModel>> GetDiscountsByProductIdAsync(int productId);
    Task<IEnumerable<DiscountModel>> GetDiscountsByCategoryIdAsync(int categoryId);
}
