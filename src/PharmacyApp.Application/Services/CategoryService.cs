using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.DTOs.Admin.ProductCategory;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;

    public CategoryService(IUnitOfWorkRepository unitOfWork, HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<PaginatedList<CategoryDto>> GetAllCategoriesAsync(int pageIndex, int pageSize)
    {
        return await _cache.GetOrCreateAsync(
            $"categories:all:{pageIndex}:{pageSize}",
            async cancel =>
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var totalCount = categories.Count();
                var items = categories.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                    .Select(c => c.ToCategoryDto()).ToList();

                return new PaginatedList<CategoryDto>
                {
                    Items = items,
                    PageIndex = pageIndex,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    PageSize = pageSize
                };
            }
        );
    }
public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
{
    return await _cache.GetOrCreateAsync(
        "categories:all",
        async cancel =>
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories.Select(c => c.ToCategoryDto()).ToList();
        }
    );
}

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        return await _cache.GetOrCreateAsync(
            $"categories:id:{categoryId}",
            async cancel =>
            {
                var categoryById = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                return categoryById != null ? categoryById.ToCategoryDto() : null;
            }
        );
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(createCategoryDto.CategoryName);

        if (existingCategory != null)
        {
            throw new ConflictException($"Category named '{createCategoryDto.CategoryName}' already exists.");
        }

        var newCategory = new CategoryModel
        {
            CategoryName = createCategoryDto.CategoryName,
            CategoryDescription = createCategoryDto.CategoryDescription
        };

        var addedCategory = await _unitOfWork.Categories.AddAsync(newCategory);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync("categories:all");

        return addedCategory.ToCategoryDto();
    }

    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(updateCategoryDto.CategoryId);

        if (category == null)
        {
            throw new NotFoundException("Category", updateCategoryDto.CategoryId);
        }

        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(updateCategoryDto.CategoryName);

        if (existingCategory != null)
        {
            throw new ConflictException($"Category named '{updateCategoryDto.CategoryName}' already exists.");
        }

        var oldName = category.CategoryName;

        category.CategoryName = updateCategoryDto.CategoryName;
        category.CategoryDescription = updateCategoryDto.CategoryDescription;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync("categories:all");
        await _cache.RemoveAsync($"categories:id:{updateCategoryDto.CategoryId}");
        await _cache.RemoveAsync($"categories:name:{oldName}");
        await _cache.RemoveAsync($"categories:name:{updateCategoryDto.CategoryName}");

        return category.ToCategoryDto();
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        if (category is null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        var hasProducts = await _unitOfWork.Products.GetAllAsync()
            .AnyAsync(p => p.CategoryId == categoryId);

        if (hasProducts)
        {
            throw new ConflictException("Cannot delete category because it has associated products.");
        }

        await _unitOfWork.Categories.DeleteAsync(categoryId);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync("categories:all");
        await _cache.RemoveAsync($"categories:id:{categoryId}");
        await _cache.RemoveAsync($"categories:name:{category.CategoryName}");
    }
}
