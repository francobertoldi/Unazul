namespace SA.Catalog.Api.ViewModels.Requirements;

public sealed record CreateRequirementRequest(
    string Name,
    string Type,
    bool IsMandatory,
    string? Description);
