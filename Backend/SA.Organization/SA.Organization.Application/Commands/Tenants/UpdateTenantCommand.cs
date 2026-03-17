using Mediator;
using SA.Organization.Application.Dtos.Tenants;

namespace SA.Organization.Application.Commands.Tenants;

public readonly record struct UpdateTenantCommand(
    Guid Id,
    string Name,
    string Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string? LogoUrl) : ICommand<TenantDto>;
