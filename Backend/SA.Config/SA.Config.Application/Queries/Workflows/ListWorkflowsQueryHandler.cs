using Mediator;
using SA.Config.Application.Dtos.Workflows;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Enums;
using Shared.Pagination;

namespace SA.Config.Application.Queries.Workflows;

public sealed class ListWorkflowsQueryHandler(
    IWorkflowRepository workflowRepository) : IQueryHandler<ListWorkflowsQuery, PagedResult<WorkflowListDto>>
{
    public async ValueTask<PagedResult<WorkflowListDto>> Handle(ListWorkflowsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        WorkflowStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<WorkflowStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var (items, total) = await workflowRepository.ListAsync(
            skip,
            pageSize,
            query.Search,
            statusFilter,
            query.SortBy,
            query.SortDir,
            ct);

        var dtos = items.Select(w => new WorkflowListDto(
            w.Id,
            w.Name,
            w.Description,
            w.Status.ToString(),
            w.Version,
            w.CreatedAt,
            w.UpdatedAt))
            .ToList();

        return new PagedResult<WorkflowListDto>(dtos, total, query.Page, pageSize);
    }
}
