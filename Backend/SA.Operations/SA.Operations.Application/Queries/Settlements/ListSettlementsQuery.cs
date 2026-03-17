using Mediator;
using SA.Operations.Application.Dtos.Settlements;
using Shared.Pagination;

namespace SA.Operations.Application.Queries.Settlements;

public readonly record struct ListSettlementsQuery(
    Guid TenantId,
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    Guid? SettledBy,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDir) : IQuery<PagedResult<SettlementListDto>>;
