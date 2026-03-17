namespace SA.Catalog.Api.ViewModels.Products;

public sealed record ProductListResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string Status,
    Guid FamilyId,
    string FamilyCode,
    string FamilyDescription,
    Guid EntityId,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    int PlanCount,
    DateTime CreatedAt);
