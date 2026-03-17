namespace SA.Catalog.Api.ViewModels.Products;

public sealed record UpdateProductRequest(
    string Name,
    string Code,
    string? Description,
    string Status,
    string ValidFrom,
    string? ValidTo);
