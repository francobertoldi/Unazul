namespace SA.Operations.Application.Dtos.Settlements;

public sealed record SettlementPreviewItemDto(
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
