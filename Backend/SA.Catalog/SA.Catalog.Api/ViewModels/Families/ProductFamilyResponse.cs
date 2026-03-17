namespace SA.Catalog.Api.ViewModels.Families;

public sealed record ProductFamilyResponse(
    Guid Id,
    string Code,
    string Description,
    string Category,
    int ProductCount,
    DateTime CreatedAt);
