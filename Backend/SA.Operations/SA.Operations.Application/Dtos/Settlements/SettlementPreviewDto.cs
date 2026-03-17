namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementPreviewDto(
    SettlementPreviewItemDto[] Items,
    SettlementTotalDto[] TotalsByCurrency);
