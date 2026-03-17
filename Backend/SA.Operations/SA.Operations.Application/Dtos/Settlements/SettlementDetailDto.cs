namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementDetailDto(
    Guid Id,
    DateTimeOffset SettledAt,
    Guid SettledBy,
    string SettledByName,
    int OperationCount,
    string? ExcelUrl,
    SettlementTotalDto[] Totals,
    SettlementItemDto[] Items);
