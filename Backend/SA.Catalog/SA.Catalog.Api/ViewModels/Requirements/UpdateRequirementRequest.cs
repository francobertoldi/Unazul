namespace SA.Catalog.Api.ViewModels.Requirements;

public sealed record UpdateRequirementRequest(
    string Name,
    string Type,
    bool IsMandatory,
    string? Description);
