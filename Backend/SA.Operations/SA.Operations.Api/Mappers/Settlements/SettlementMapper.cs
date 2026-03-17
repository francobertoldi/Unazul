using SA.Operations.Api.ViewModels.Settlements;
using SA.Operations.Domain.Entities;

namespace SA.Operations.Api.Mappers.Settlements;

public static class SettlementMapper
{
    public static SettlementListResponse ToListResponse(Settlement s)
    {
        return new SettlementListResponse(
            s.Id,
            s.SettledAt,
            s.SettledByName,
            s.OperationCount,
            s.Totals.Select(ToTotalResponse).ToList());
    }

    public static SettlementDetailResponse ToDetailResponse(Settlement s)
    {
        return new SettlementDetailResponse(
            s.Id,
            s.SettledAt,
            s.SettledBy,
            s.SettledByName,
            s.OperationCount,
            s.ExcelUrl,
            s.Items.Select(ToItemResponse).ToList(),
            s.Totals.Select(ToTotalResponse).ToList());
    }

    public static SettlementItemResponse ToItemResponse(SettlementItem item)
    {
        return new SettlementItemResponse(
            item.Id,
            item.ApplicationId,
            item.AppCode,
            item.ApplicantName,
            item.ProductName,
            item.PlanName,
            item.CommissionType,
            item.CommissionValue,
            item.CalculatedAmount,
            item.Currency,
            item.FormulaDescription);
    }

    public static SettlementTotalResponse ToTotalResponse(SettlementTotal total)
    {
        return new SettlementTotalResponse(
            total.Currency,
            total.TotalAmount,
            total.ItemCount);
    }
}
