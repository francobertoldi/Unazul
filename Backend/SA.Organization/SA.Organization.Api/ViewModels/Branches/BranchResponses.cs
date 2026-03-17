namespace SA.Organization.Api.ViewModels.Branches;

public sealed record BranchResponse(
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
