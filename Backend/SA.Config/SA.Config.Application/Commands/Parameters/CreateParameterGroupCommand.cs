using Mediator;
using SA.Config.Application.Dtos.Parameters;

namespace SA.Config.Application.Commands.Parameters;

public readonly record struct CreateParameterGroupCommand(
    string Code,
    string Name,
    string Category,
    string Icon,
    int SortOrder) : ICommand<ParameterGroupDto>;
