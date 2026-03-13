using PharmacyApp.Application.DTOs.Admin.ProductCategory;
using PharmacyApp.Application.DTOs.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<PaginatedList<CategoryDto>> GetAllCategoriesAsync(int pageIndex, int pageSize);
    Task<CategoryDto?> GetCategoryByNameAsync(string categoryName);
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);
    //Task<CategoryDetailsDto> GetCategoryWithProductsAsync(string categoryName);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto); 
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);
    Task DeleteCategoryAsync(int categoryId);
}
