namespace SA.Organization.Application.Dtos.Tenants;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Identifier,
    string Status,
    string? Address,
    string? City,
    string? Province,
    string? Phone,
    string? Email,
    DateTime CreatedAt);
