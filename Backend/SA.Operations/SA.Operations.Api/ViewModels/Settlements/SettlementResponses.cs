namespace SA.Operations.Api.ViewModels.Settlements;

public sealed record SettlementPreviewResponse(
    int OperationCount,
    IReadOnlyList<SettlementPreviewItemResponse> Items,
    IReadOnlyList<SettlementTotalResponse> Totals);

public sealed record SettlementPreviewItemResponse(
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

public sealed record SettlementListResponse(
    Guid Id,
    DateTime SettledAt,
    string SettledByName,
    int OperationCount,
    IReadOnlyList<SettlementTotalResponse> Totals);

public sealed record SettlementDetailResponse(
    Guid Id,
    DateTime SettledAt,
    Guid SettledBy,
    string SettledByName,
    int OperationCount,
    string? ExcelUrl,
    IReadOnlyList<SettlementItemResponse> Items,
    IReadOnlyList<SettlementTotalResponse> Totals);

public sealed record SettlementItemResponse(
    Guid Id,
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

public sealed record SettlementTotalResponse(
    string Currency,
    decimal TotalAmount,
    int ItemCount);
