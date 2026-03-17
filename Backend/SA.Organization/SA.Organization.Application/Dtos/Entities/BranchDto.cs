namespace SA.Organization.Application.Dtos.Entities;

public sealed record BranchDto(
    Guid Id,
    Guid EntityId,
    string Name,
    string Code,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
