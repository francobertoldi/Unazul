namespace SA.Config.Api.ViewModels.Parameters;

public sealed record CategoryResponse(
    string Name,
    IReadOnlyList<ParameterGroupResponse> Groups);

public sealed record ParameterGroupResponse(
    Guid Id,
    string Code,
    string Name,
    string Icon,
    int SortOrder);
