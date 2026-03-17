using Mediator;
using SA.Operations.Application.Dtos.Settlements;

namespace SA.Operations.Application.Commands.Settlements;

public readonly record struct PreviewSettlementCommand(
    Guid TenantId,
    Guid? EntityId,
    DateTime DateFrom,
    DateTime DateTo) : ICommand<SettlementPreviewDto>;
