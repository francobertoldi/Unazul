using Mediator;

namespace SA.Config.Application.Commands.Parameters;

public readonly record struct DeleteParameterCommand(Guid Id) : ICommand;
