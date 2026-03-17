using Mediator;
using SA.Catalog.Application.Dtos;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Families;

public readonly record struct ListProductFamiliesQuery(
    Guid TenantId, int Page, int PageSize,
    string? Search) : IQuery<PagedResult<ProductFamilyDto>>;
