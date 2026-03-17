using SA.Config.Domain.Enums;

namespace SA.Config.Api.ViewModels.Parameters;

public sealed record ParameterResponse(
    Guid Id,
    string Key,
    string Value,
    ParameterType Type,
    string Description,
    string? ParentKey,
    IReadOnlyList<ParameterOptionResponse> Options);

public sealed record ParameterOptionResponse(
    Guid Id,
    string OptionValue,
    string OptionLabel,
    int SortOrder);
