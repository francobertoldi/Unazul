namespace SA.Config.Application.Dtos.Parameters;

public sealed record CategoryDto(
    string Name,
    IReadOnlyList<ParameterGroupDto> Groups);
