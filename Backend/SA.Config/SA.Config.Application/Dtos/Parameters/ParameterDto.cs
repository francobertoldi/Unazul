using SA.Config.Domain.Enums;

namespace SA.Config.Application.Dtos.Parameters;

public sealed record ParameterDto(
    Guid Id,
    string Key,
    string Value,
    ParameterType Type,
    string Description,
    string? ParentKey,
    IReadOnlyList<ParameterOptionDto> Options);
