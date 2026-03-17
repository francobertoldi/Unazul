using Mediator;

namespace SA.Catalog.Application.Commands.Plans;

public readonly record struct DeleteProductPlanCommand(Guid TenantId, Guid Id) : ICommand;
