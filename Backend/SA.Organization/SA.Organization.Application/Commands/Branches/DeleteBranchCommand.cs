using Mediator;

namespace SA.Organization.Application.Commands.Branches;

public readonly record struct DeleteBranchCommand(Guid EntityId, Guid Id, Guid DeletedBy) : ICommand;
