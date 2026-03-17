namespace SA.Config.Application.Dtos.Parameters;

public sealed record ParameterGroupDto(
    Guid Id,
    string Code,
    string Name,
    string Icon,
    int SortOrder);
