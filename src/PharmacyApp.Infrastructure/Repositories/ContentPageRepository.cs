using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class ContentPageRepository : IContentPageRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public ContentPageRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<ContentPage> Query()
    {
        return _dbContext.Set<ContentPage>().AsNoTracking();
    }

    public async Task<ContentPage?> GetBySlugAsync(string slug)
    {
        return await _dbContext.Set<ContentPage>()
            .FirstOrDefaultAsync(page => page.Slug == slug);
    }

    public async Task AddAsync(ContentPage page)
    {
        await _dbContext.Set<ContentPage>().AddAsync(page);
    }

    public Task UpdateAsync(ContentPage page)
    {
        _dbContext.Set<ContentPage>().Update(page);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ContentPage page)
    {
        _dbContext.Set<ContentPage>().Remove(page);
        return Task.CompletedTask;
    }
}
