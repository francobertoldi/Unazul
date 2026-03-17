namespace SA.Config.Api.ViewModels.Parameters;

public sealed record CreateParameterGroupRequest(
    string Code,
    string Name,
    string Category,
    string Icon,
    int SortOrder);
