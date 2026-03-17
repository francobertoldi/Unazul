using Mediator;

namespace SA.Organization.Application.Commands.Tenants;

public readonly record struct DeleteTenantCommand(Guid Id, Guid DeletedBy) : ICommand;
