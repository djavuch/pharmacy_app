using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IContentPageRepository
{
    IQueryable<ContentPage> Query();
    Task<ContentPage?> GetBySlugAsync(string slug);
    Task AddAsync(ContentPage page);
    Task UpdateAsync(ContentPage page);
    Task DeleteAsync(ContentPage page);
}
