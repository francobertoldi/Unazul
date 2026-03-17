using Mediator;
using SA.Config.Application.Dtos.Parameters;

namespace SA.Config.Application.Commands.Parameters;

public readonly record struct UpdateParameterCommand(
    Guid Id,
    string Value,
    UpdateParameterOptionInput[]? Options,
    Guid UpdatedBy) : ICommand<ParameterDto>;

public sealed record UpdateParameterOptionInput(string OptionValue, string OptionLabel);
