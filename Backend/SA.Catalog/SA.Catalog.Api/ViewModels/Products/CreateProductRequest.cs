namespace SA.Catalog.Api.ViewModels.Products;

public sealed record CreateProductRequest(
    Guid EntityId,
    Guid FamilyId,
    string Name,
    string Code,
    string? Description,
    string Status,
    string ValidFrom,
    string? ValidTo);
