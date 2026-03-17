namespace SA.Organization.Application.Dtos.Entities;

public sealed record EntityDto(
    Guid Id,
    string Name,
    string Cuit,
    string Type,
    string Status,
    string? City,
    string? Province,
    DateTime CreatedAt);
