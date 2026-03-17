namespace SA.Operations.Api.ViewModels.Settlements;

public sealed record SettlementPreviewRequest(
    Guid? EntityId,
    DateTime DateFrom,
    DateTime DateTo);

public sealed record ConfirmSettlementRequest(
    Guid? EntityId,
    DateTime DateFrom,
    DateTime DateTo);
