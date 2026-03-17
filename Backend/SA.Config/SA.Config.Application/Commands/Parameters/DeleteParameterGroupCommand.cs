using Mediator;

namespace SA.Config.Application.Commands.Parameters;

public readonly record struct DeleteParameterGroupCommand(Guid Id) : ICommand;
