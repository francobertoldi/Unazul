using Mediator;

namespace SA.Organization.Application.Commands.Entities;

public readonly record struct DeleteEntityCommand(Guid Id, Guid DeletedBy) : ICommand;
