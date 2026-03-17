namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementItemDto(
    Guid Id,
    Guid SettlementId,
    Guid ApplicationId,
    string AppCode,
    string ApplicantName,
    string ProductName,
    string PlanName,
    string? CommissionType,
    decimal? CommissionValue,
    decimal CalculatedAmount,
    string Currency,
    string? FormulaDescription);
