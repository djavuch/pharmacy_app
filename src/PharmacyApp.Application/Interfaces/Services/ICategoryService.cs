using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Category;
using PharmacyApp.Application.Contracts.Category.Admin;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<PaginatedList<CategoryDto>> GetAllCategoriesAsync(QueryParams query);
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);
    Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto); 
    Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);
    Task<Result> DeleteCategoryAsync(int categoryId);
}
