using Mediator;
using SA.Catalog.Application.Dtos;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Commissions;

public sealed class ListCommissionPlansQueryHandler(
    ICommissionPlanRepository commissionPlanRepository) : IQueryHandler<ListCommissionPlansQuery, PagedResult<CommissionPlanDto>>
{
    public async ValueTask<PagedResult<CommissionPlanDto>> Handle(ListCommissionPlansQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        var (plans, total) = await commissionPlanRepository.ListAsync(
            skip, pageSize, query.Search, ct);

        var planIds = plans.Select(p => p.Id);
        var assignedCounts = await commissionPlanRepository.CountAssignedPlansBatchAsync(planIds, ct);

        var items = plans.Select(plan => new CommissionPlanDto(
            plan.Id, plan.Code, plan.Description,
            plan.Type.ToString().ToLowerInvariant(),
            plan.Value, plan.MaxAmount,
            assignedCounts.GetValueOrDefault(plan.Id, 0), plan.CreatedAt))
            .ToList();

        return new PagedResult<CommissionPlanDto>(items, total, query.Page, pageSize);
    }
}
