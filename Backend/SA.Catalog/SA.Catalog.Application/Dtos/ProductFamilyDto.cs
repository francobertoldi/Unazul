namespace SA.Catalog.Application.Dtos;

public sealed record ProductFamilyDto(
    Guid Id, string Code, string Description, string Category,
    int ProductCount, DateTime CreatedAt);
