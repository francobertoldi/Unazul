namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementTotalDto(
    string Currency,
    decimal TotalAmount,
    int ItemCount);
