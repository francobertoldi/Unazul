namespace SA.Organization.Api.ViewModels.Entities;

public sealed record CreateEntityRequest(
    string Name,
    string Cuit,
    string Type,
    string? Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string[]? Channels);

public sealed record UpdateEntityRequest(
    string Name,
    string Type,
    string? Status,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Country,
    string? Phone,
    string? Email,
    string[]? Channels);
