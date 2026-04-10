namespace PharmacyApp.Application.Common.Pagination;

public record ReviewQueryParams : QueryParams
{
    public bool? IsApproved { get; init; }
}