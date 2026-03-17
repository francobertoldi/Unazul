using Mediator;

namespace SA.Catalog.Application.Commands.Requirements;

public readonly record struct DeleteRequirementCommand(Guid TenantId, Guid Id, Guid ProductId) : ICommand;
