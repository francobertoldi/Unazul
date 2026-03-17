using Mediator;

namespace SA.Catalog.Application.Commands.Commissions;

public readonly record struct DeleteCommissionPlanCommand(Guid TenantId, Guid Id) : ICommand;
