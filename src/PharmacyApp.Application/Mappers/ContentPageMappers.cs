using PharmacyApp.Application.Contracts.Content;
using PharmacyApp.Application.Contracts.Content.Admin;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ContentPageMappers
{
    public static ContentPageDto ToPublicDto(this ContentPage page) => new()
    {
        Slug = page.Slug,
        Title = page.Title,
        Content = page.Content,
        UpdatedAt = page.UpdatedAt
    };

    public static AdminContentPageDto ToAdminDto(this ContentPage page) => new()
    {
        Id = page.Id,
        Slug = page.Slug,
        Title = page.Title,
        Content = page.Content,
        IsPublished = page.IsPublished,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        UpdatedBy = page.UpdatedBy
    };
}
