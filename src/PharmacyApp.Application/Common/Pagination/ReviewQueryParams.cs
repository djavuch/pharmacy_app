using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Common.Pagination;

public record ReviewQueryParams : QueryParams
{
    public ReviewStatus? Status { get; init; }
}
