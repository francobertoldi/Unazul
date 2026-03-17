using Mediator;
using SA.Catalog.Application.Dtos;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Commissions;

public readonly record struct ListCommissionPlansQuery(
    Guid TenantId, int Page, int PageSize,
    string? Search) : IQuery<PagedResult<CommissionPlanDto>>;
