using Mediator;

namespace SA.Operations.Application.Commands.Settlements;

public readonly record struct ConfirmSettlementCommand(
    Guid TenantId,
    Guid? EntityId,
    DateTime DateFrom,
    DateTime DateTo,
    Guid SettledBy,
    string SettledByName) : ICommand<ConfirmSettlementResult>;

public sealed record ConfirmSettlementResult(Guid SettlementId, int ItemsCount, string? ExcelUrl);
