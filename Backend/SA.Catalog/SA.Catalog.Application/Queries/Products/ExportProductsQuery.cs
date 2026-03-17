using Mediator;

namespace SA.Catalog.Application.Queries.Products;

public readonly record struct ExportProductsQuery(
    Guid TenantId, string Format,
    string? Search, string? Status, Guid? FamilyId, Guid? EntityId)
    : IQuery<(byte[] Data, string ContentType, string FileName)>;
