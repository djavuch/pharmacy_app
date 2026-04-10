using Microsoft.EntityFrameworkCore;

namespace PharmacyApp.Application.Common.Pagination;

public record PaginatedList<T>
{
    public List<T> Items { get; set; } = [];
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }   
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return new PaginatedList<T>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize)
        };
    }
    
    public static Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, QueryParams query) => 
        CreateAsync(source, query.PageIndex, query.PageSize);

    public static PaginatedList<T> Create(List<T> items, int totalCount, QueryParams query)
        => new()
        {
            Items = items,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
}
