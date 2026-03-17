using Mediator;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Application.Commands.Branches;

public readonly record struct CreateBranchCommand(
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
    bool IsActive) : ICommand<BranchDto>;
