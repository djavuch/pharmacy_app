using PharmacyApp.Application.Contracts.Content;
using PharmacyApp.Application.Contracts.Content.Admin;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IContentPageService
{
    Task<IReadOnlyCollection<AdminContentPageDto>> GetAllAsync();
    Task<Result<AdminContentPageDto>> GetBySlugForAdminAsync(string slug);
    Task<Result<ContentPageDto>> GetPublishedBySlugAsync(string slug);
    Task<Result<AdminContentPageDto>> UpdateAsync(string slug, UpdateContentPageDto dto, string? updatedBy);
}
