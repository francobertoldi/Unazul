using Mediator;

namespace SA.Organization.Application.Queries.Tenants;

public readonly record struct ExportTenantsQuery(
    string Format,
    string? Search,
    string? Status) : IQuery<byte[]>;
