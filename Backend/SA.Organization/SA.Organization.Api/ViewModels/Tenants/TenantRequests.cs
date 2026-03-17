namespace SA.Organization.Api.ViewModels.Tenants;

public sealed record CreateTenantRequest(
    string Name,
    string Identifier,
    string? Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string? LogoUrl);

public sealed record UpdateTenantRequest(
    string Name,
    string? Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string? LogoUrl);
