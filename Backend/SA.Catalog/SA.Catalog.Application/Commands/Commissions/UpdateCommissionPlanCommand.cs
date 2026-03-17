using Mediator;

namespace SA.Catalog.Application.Commands.Commissions;

public readonly record struct UpdateCommissionPlanCommand(
    Guid TenantId, Guid Id,
    string Code, string Description,
    string Type, decimal Value, decimal? MaxAmount,
    Guid UserId) : ICommand;
