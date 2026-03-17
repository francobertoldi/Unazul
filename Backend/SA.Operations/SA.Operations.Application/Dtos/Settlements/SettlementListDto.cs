namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementListDto(
    Guid Id,
    DateTimeOffset SettledAt,
    string SettledByName,
    int OperationCount,
    string? ExcelUrl,
    SettlementTotalDto[] Totals);
