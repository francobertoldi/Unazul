namespace SA.Organization.Api.ViewModels.Tenants;

public sealed record TenantListResponse(
    Guid Id,
    string Name,
    string Identifier,
    string Status,
    string? Phone,
    string? Email,
    DateTime CreatedAt);

public sealed record TenantDetailResponse(
    Guid Id,
    string Name,
    string Identifier,
    string Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string? LogoUrl,
    int EntityCount,
    int UserCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
