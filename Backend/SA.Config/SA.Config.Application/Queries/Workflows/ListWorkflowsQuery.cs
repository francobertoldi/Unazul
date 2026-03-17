using Mediator;
using SA.Config.Application.Dtos.Workflows;
using Shared.Pagination;

namespace SA.Config.Application.Queries.Workflows;

public readonly record struct ListWorkflowsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? SortBy,
    string? SortDir) : IQuery<PagedResult<WorkflowListDto>>;
