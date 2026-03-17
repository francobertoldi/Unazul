using Mediator;
using SA.Operations.Application.Dtos.Settlements;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Operations.Application.Queries.Settlements;

public sealed class ListSettlementsQueryHandler(
    ISettlementRepository settlementRepository) : IQueryHandler<ListSettlementsQuery, PagedResult<SettlementListDto>>
{
    public async ValueTask<PagedResult<SettlementListDto>> Handle(ListSettlementsQuery query, CancellationToken ct)
    {
        var pagination = new PaginationRequest(query.Page, query.PageSize, query.SortBy, query.SortDir ?? "desc");

        var result = await settlementRepository.ListAsync(
            pagination.Skip,
            pagination.ClampedPageSize,
            query.TenantId,
            query.DateFrom,
            query.DateTo,
            query.SettledBy,
            pagination.Sort,
            query.SortDir ?? "desc",
            ct);

        var dtos = result.Items.Select(s => new SettlementListDto(
            s.Id,
            s.SettledAt,
            s.SettledByName,
            s.OperationCount,
            s.ExcelUrl,
            s.Totals.Select(t => new SettlementTotalDto(t.Currency, t.TotalAmount, t.ItemCount)).ToArray()
        )).ToList();

        return new PagedResult<SettlementListDto>(dtos, result.Total, query.Page, query.PageSize);
    }
}
