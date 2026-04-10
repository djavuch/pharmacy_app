using PharmacyApp.Application.Contracts.Category;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class CategoryMappers
{
    public static CategoryDto ToCategoryDto(this Category category) => new()
    {
        CategoryId = category.CategoryId,
        CategoryName = category.CategoryName,
        CategoryDescription = category.CategoryDescription
    };
}
