using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Export;

namespace SA.Catalog.Application.Queries.Products;

public sealed class ExportProductsQueryHandler(
    IProductRepository productRepository) : IQueryHandler<ExportProductsQuery, (byte[] Data, string ContentType, string FileName)>
{
    private static readonly IReadOnlyList<ColumnDefinition<Product>> Columns =
    [
        new("Nombre", p => p.Name),
        new("Codigo", p => p.Code),
        new("Estado", p => p.Status.ToString().ToLowerInvariant()),
        new("Familia", p => p.Family?.Description ?? string.Empty),
        new("Entidad", p => p.EntityId),
        new("Vigencia Desde", p => p.ValidFrom),
        new("Vigencia Hasta", p => p.ValidTo),
        new("Planes", p => p.Plans.Count),
        new("Creado", p => p.CreatedAt)
    ];

    public async ValueTask<(byte[] Data, string ContentType, string FileName)> Handle(
        ExportProductsQuery query, CancellationToken ct)
    {
        Shared.Contract.Enums.ProductStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<Shared.Contract.Enums.ProductStatus>(query.Status, true, out var s))
            parsedStatus = s;
        var excludeDeprecated = !parsedStatus.HasValue;

        var totalCount = await productRepository.CountForExportAsync(
            query.Search, parsedStatus, query.FamilyId, query.EntityId, excludeDeprecated, ct);

        if (totalCount > 10_000)
            throw new InvalidOperationException("CAT_EXPORT_LIMIT_EXCEEDED");

        var products = await productRepository.ListForExportAsync(
            query.Search, parsedStatus, query.FamilyId, query.EntityId, excludeDeprecated, ct);

        var isCsv = query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase);

        var data = isCsv
            ? ExportService.ToCsv(products, Columns)
            : ExportService.ToXlsx(products, Columns, "Productos");

        var contentType = isCsv
            ? "text/csv"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileName = isCsv ? "productos.csv" : "productos.xlsx";

        return (data, contentType, fileName);
    }
}
