using Mediator;

namespace SA.Catalog.Application.Commands.Commissions;

public readonly record struct CreateCommissionPlanCommand(
    Guid TenantId, string Code, string Description,
    string Type, decimal Value, decimal? MaxAmount,
    Guid UserId) : ICommand<Guid>;
