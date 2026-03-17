namespace SA.Organization.Application.Dtos.Entities;

public sealed record EntityDetailDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Cuit,
    string Type,
    string Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    IReadOnlyList<EntityChannelDto> Channels,
    IReadOnlyList<BranchDto> Branches,
    DateTime CreatedAt,
    DateTime UpdatedAt);
