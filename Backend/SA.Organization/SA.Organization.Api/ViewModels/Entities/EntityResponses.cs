namespace SA.Organization.Api.ViewModels.Entities;

public sealed record EntityListResponse(
    Guid Id,
    string Name,
    string Cuit,
    string Type,
    string Status,
    string? City,
    string? Province,
    DateTime CreatedAt);

public sealed record EntityDetailResponse(
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
    IReadOnlyList<EntityChannelResponse> Channels,
    IReadOnlyList<EntityBranchResponse> Branches,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record EntityChannelResponse(
    Guid Id,
    string ChannelType,
    bool IsActive);

public sealed record EntityBranchResponse(
    Guid Id,
    string Name,
    string Code,
    bool IsActive);
