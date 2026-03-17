using Mediator;

namespace SA.Catalog.Application.Commands.Coverages;

public readonly record struct DeleteCoverageCommand(Guid TenantId, Guid Id) : ICommand;
