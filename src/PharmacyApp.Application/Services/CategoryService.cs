using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Category;
using PharmacyApp.Application.Contracts.Category.Admin;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;

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

    public async Task<PaginatedList<CategoryDto>> GetAllCategoriesAsync(QueryParams query)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Categories.All(query.PageIndex, query.PageSize),
            async _ =>
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                
                var totalCount = categories.Count();
                
                var items =  categories
                    .Skip((query.PageIndex - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(c => c.ToCategoryDto()).ToList();

                return PaginatedList<CategoryDto>.Create(items, totalCount, query);
            }
        );
    }
    
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Categories.ById(categoryId),
            async _ =>
            {
                var categoryById = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                return categoryById.ToCategoryDto();
            }
        );
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(createCategoryDto.CategoryName);

        if (existingCategory is not null)
            return Result<CategoryDto>.Conflict($"Category named '{createCategoryDto.CategoryName}' already exists.");
        
        var newCategory = new Category(createCategoryDto.CategoryName, createCategoryDto.CategoryDescription);

        var addedCategory = await _unitOfWork.Categories.AddAsync(newCategory);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Categories.AllPrefix);

        return Result<CategoryDto>.Success(addedCategory.ToCategoryDto());
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(updateCategoryDto.CategoryId);

        if (category is null)
            return Result<CategoryDto>.NotFound($"Category named '{updateCategoryDto.CategoryName}' does not exist.");

        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(updateCategoryDto.CategoryName);

        if (existingCategory is not null)
           return Result<CategoryDto>.Conflict($"Category named '{updateCategoryDto.CategoryName}' already exists.");
        
        var oldName = category.CategoryName;

        category.Update(updateCategoryDto.CategoryName, updateCategoryDto.CategoryDescription);

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Categories.AllPrefix);
        await _cache.RemoveAsync(CacheKeys.Categories.ById(updateCategoryDto.CategoryId));
        await _cache.RemoveAsync(CacheKeys.Categories.ByName(oldName));
        await _cache.RemoveAsync(CacheKeys.Categories.ByName(updateCategoryDto.CategoryName));

        return Result<CategoryDto>.Success(category.ToCategoryDto());
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        if (category is null)
            return Result.NotFound($"Category with ID '{categoryId}' does not exist.");

        var hasProducts = await _unitOfWork.Products.GetAllAsync()
            .AnyAsync(p => p.CategoryId == categoryId);

        if (hasProducts)
            return Result.Conflict("Cannot delete category because it has associated products.");

        await _unitOfWork.Categories.DeleteAsync(categoryId);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Categories.AllPrefix);
        await _cache.RemoveAsync(CacheKeys.Categories.ById(categoryId));
        await _cache.RemoveAsync(CacheKeys.Categories.ByName(category.CategoryName));
        
        return Result.Success();
    }
}
