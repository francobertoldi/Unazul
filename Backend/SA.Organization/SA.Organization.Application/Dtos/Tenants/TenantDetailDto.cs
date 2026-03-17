namespace SA.Organization.Application.Dtos.Tenants;

public sealed record TenantDetailDto(
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
