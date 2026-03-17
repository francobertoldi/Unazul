using Mediator;
using SA.Organization.Application.Dtos.Tenants;
using Shared.Pagination;

namespace SA.Organization.Application.Queries.Tenants;

public readonly record struct ListTenantsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Sort,
    string Order) : IQuery<PagedResult<TenantDto>>;
