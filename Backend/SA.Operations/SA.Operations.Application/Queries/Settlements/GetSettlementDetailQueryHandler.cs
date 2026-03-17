using Mediator;
using SA.Operations.Application.Dtos.Settlements;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Queries.Settlements;

public sealed class GetSettlementDetailQueryHandler(
    ISettlementRepository settlementRepository) : IQueryHandler<GetSettlementDetailQuery, SettlementDetailDto>
{
    public async ValueTask<SettlementDetailDto> Handle(GetSettlementDetailQuery query, CancellationToken ct)
    {
        var settlement = await settlementRepository.GetByIdWithDetailsAsync(query.SettlementId, ct);
        if (settlement is null || settlement.TenantId != query.TenantId)
            throw new NotFoundException("OPS_SETTLEMENT_NOT_FOUND", "Liquidacion no encontrada.");

        return new SettlementDetailDto(
            settlement.Id,
            settlement.SettledAt,
            settlement.SettledBy,
            settlement.SettledByName,
            settlement.OperationCount,
            settlement.ExcelUrl,
            settlement.Totals.Select(t => new SettlementTotalDto(t.Currency, t.TotalAmount, t.ItemCount)).ToArray(),
            settlement.Items.Select(i => new SettlementItemDto(
                i.Id,
                i.SettlementId,
                i.ApplicationId,
                i.AppCode,
                i.ApplicantName,
                i.ProductName,
                i.PlanName,
                i.CommissionType,
                i.CommissionValue,
                i.CalculatedAmount,
                i.Currency,
                i.FormulaDescription)).ToArray());
    }
}
