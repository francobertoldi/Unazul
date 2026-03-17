using SA.Catalog.Api.ViewModels.Plans;
using SA.Catalog.Api.ViewModels.Requirements;

namespace SA.Catalog.Api.ViewModels.Products;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string Status,
    Guid EntityId,
    Guid FamilyId,
    string FamilyCode,
    string FamilyDescription,
    string Category,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PlanResponse> Plans,
    IReadOnlyList<RequirementResponse> Requirements);
