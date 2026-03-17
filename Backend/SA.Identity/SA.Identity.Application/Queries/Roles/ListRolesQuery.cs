using Mediator;
using SA.Identity.Application.Dtos.Roles;
using Shared.Pagination;

namespace SA.Identity.Application.Queries.Roles;

public readonly record struct ListRolesQuery(
    Guid TenantId,
    int Page,
    int PageSize,
    string? Search,
    string? SortBy,
    string? SortDir,
    bool Export) : IQuery<PagedResult<RoleDto>>;
