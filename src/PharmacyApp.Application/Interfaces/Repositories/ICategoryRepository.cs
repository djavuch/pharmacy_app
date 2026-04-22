using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    IQueryable<Category> Query();
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int categoryId);
    Task<Category?> GetByNameAsync(string categoryName);
    //Task<CategoryModel> GetCategoryWithProductsAsync(string categoryName);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int categoryId);
}
