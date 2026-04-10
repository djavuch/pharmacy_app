namespace PharmacyApp.Application.Common.Pagination;

public record QueryParams
{
    public int PageIndex { get; init; } = 1;
    public int PageSize  { get; init; } = 10;
    public string? FilterOn    { get; init; }
    public string? FilterQuery { get; init; }
    public string? SortBy      { get; init; }
    public bool IsAscending    { get; init; } = true;
}