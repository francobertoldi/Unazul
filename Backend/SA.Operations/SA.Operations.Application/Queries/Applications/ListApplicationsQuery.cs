using Mediator;
using SA.Operations.Application.Dtos.Applications;
using Shared.Pagination;

namespace SA.Operations.Application.Queries.Applications;

public readonly record struct ListApplicationsQuery(
    Guid TenantId,
    string? Status,
    Guid? EntityId,
    string? Search,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDir,
    bool Export) : IQuery<PagedResult<ApplicationListDto>>;
