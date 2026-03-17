using Mediator;

namespace SA.Organization.Application.Queries.Entities;

public readonly record struct ExportEntitiesQuery(
    string Format,
    string? Search,
    string? Status,
    string? Type) : IQuery<byte[]>;
