using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Contracts.Content;
using PharmacyApp.Application.Contracts.Content.Admin;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Services;

public class ContentPageService : IContentPageService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private static readonly string[] ManagedSlugs =
    [
        "contacts",
        "license-agreement",
        "about"
    ];

    public ContentPageService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<AdminContentPageDto>> GetAllAsync()
    {
        var pages = await _unitOfWork.ContentPages.Query()
            .Where(page => ManagedSlugs.Contains(page.Slug))
            .ToListAsync();

        var pageBySlug = pages.ToDictionary(page => page.Slug, StringComparer.OrdinalIgnoreCase);

        var orderedPages = ManagedSlugs
            .Where(slug => pageBySlug.ContainsKey(slug))
            .Select(slug => pageBySlug[slug].ToAdminDto())
            .ToList();

        return orderedPages;
    }

    public async Task<Result<AdminContentPageDto>> GetBySlugForAdminAsync(string slug)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
            return Result<AdminContentPageDto>.BadRequest("Slug is required.");
        if (!IsManagedSlug(normalizedSlug))
            return Result<AdminContentPageDto>.BadRequest($"Unsupported content slug '{normalizedSlug}'.");

        var page = await _unitOfWork.ContentPages.GetBySlugAsync(normalizedSlug);
        if (page is null)
            return Result<AdminContentPageDto>.NotFound($"Content page '{normalizedSlug}' not found.");

        return Result<AdminContentPageDto>.Success(page.ToAdminDto());
    }

    public async Task<Result<ContentPageDto>> GetPublishedBySlugAsync(string slug)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
            return Result<ContentPageDto>.BadRequest("Slug is required.");
        if (!IsManagedSlug(normalizedSlug))
            return Result<ContentPageDto>.BadRequest($"Unsupported content slug '{normalizedSlug}'.");

        var page = await _unitOfWork.ContentPages.GetBySlugAsync(normalizedSlug);
        if (page is null || !page.IsPublished)
            return Result<ContentPageDto>.NotFound($"Content page '{normalizedSlug}' not found.");

        return Result<ContentPageDto>.Success(page.ToPublicDto());
    }

    public async Task<Result<AdminContentPageDto>> UpdateAsync(string slug, UpdateContentPageDto dto, string? updatedBy)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
            return Result<AdminContentPageDto>.BadRequest("Slug is required.");
        if (!IsManagedSlug(normalizedSlug))
            return Result<AdminContentPageDto>.BadRequest($"Unsupported content slug '{normalizedSlug}'.");

        var page = await _unitOfWork.ContentPages.GetBySlugAsync(normalizedSlug);
        if (page is null)
            return Result<AdminContentPageDto>.NotFound($"Content page '{normalizedSlug}' not found.");

        page.Update(dto.Title.Trim(), dto.Content.Trim(), dto.IsPublished, updatedBy);
        await _unitOfWork.ContentPages.UpdateAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return Result<AdminContentPageDto>.Success(page.ToAdminDto());
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }

    private static bool IsManagedSlug(string slug)
    {
        return ManagedSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase);
    }
}
