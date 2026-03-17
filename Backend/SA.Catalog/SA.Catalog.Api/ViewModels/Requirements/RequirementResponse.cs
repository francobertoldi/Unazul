namespace SA.Catalog.Api.ViewModels.Requirements;

public sealed record RequirementResponse(
    Guid Id,
    string Name,
    string Type,
    bool IsMandatory,
    string? Description);
