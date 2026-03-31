using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<CategoryModel>> GetAllAsync();
    Task<CategoryModel> GetByIdAsync(int categoryId);
    Task<CategoryModel?> GetByNameAsync(string categoryName);
    //Task<CategoryModel> GetCategoryWithProductsAsync(string categoryName);
    Task<CategoryModel> AddAsync(CategoryModel category);
    Task UpdateAsync(CategoryModel category);
    Task DeleteAsync(int categoryId);
}