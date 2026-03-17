using SA.Config.Domain.Enums;

namespace SA.Config.Api.ViewModels.Parameters;

public sealed record CreateParameterRequest(
    Guid GroupId,
    string Key,
    string Value,
    ParameterType Type,
    string Description,
    string? ParentKey,
    CreateParameterOptionRequest[]? Options);

public sealed record CreateParameterOptionRequest(string OptionValue, string OptionLabel);

public sealed record UpdateParameterRequest(
    string Value,
    UpdateParameterOptionRequest[]? Options);

public sealed record UpdateParameterOptionRequest(string OptionValue, string OptionLabel);
