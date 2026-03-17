namespace SA.Organization.Api.ViewModels.Branches;

public sealed record CreateBranchRequest(
    string Name,
    string Code,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    bool IsActive);

public sealed record UpdateBranchRequest(
    string Name,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    bool IsActive);
