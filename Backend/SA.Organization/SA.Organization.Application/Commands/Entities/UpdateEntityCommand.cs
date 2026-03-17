using Mediator;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Application.Commands.Entities;

public readonly record struct UpdateEntityCommand(
    Guid Id,
    string Name,
    string Type,
    string Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string[]? Channels) : ICommand<EntityDetailDto>;
