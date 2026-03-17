using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.Domain.Enums;

namespace SA.Config.Application.Commands.Parameters;

public readonly record struct CreateParameterCommand(
    Guid TenantId,
    Guid GroupId,
    string Key,
    string Value,
    ParameterType Type,
    string Description,
    string? ParentKey,
    CreateParameterOptionInput[]? Options,
    Guid CreatedBy) : ICommand<ParameterDto>;

public sealed record CreateParameterOptionInput(string OptionValue, string OptionLabel);
