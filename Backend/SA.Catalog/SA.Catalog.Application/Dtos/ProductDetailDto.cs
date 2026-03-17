namespace SA.Catalog.Application.Dtos;

public sealed record ProductDetailDto(
    Guid Id, string Name, string Code, string? Description, string Status,
    Guid EntityId, Guid FamilyId, string FamilyCode, string FamilyDescription, string Category,
    DateOnly ValidFrom, DateOnly? ValidTo, int Version,
    DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<ProductPlanDto> Plans,
    IReadOnlyList<ProductRequirementDto> Requirements);
