using PharmacyApp.Application.DTOs.Admin.ProductCategory;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class CategoryMappers
{
    public static CategoryDto ToCategoryDto(this CategoryModel category) => new()
    {
        CategoryId = category.CategoryId,
        CategoryName = category.CategoryName,
        CategoryDescription = category.CategoryDescription
    };
}
