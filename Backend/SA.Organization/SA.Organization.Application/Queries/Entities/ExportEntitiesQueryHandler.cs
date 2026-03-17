using Mediator;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Exceptions;
using Shared.Export;

namespace SA.Organization.Application.Queries.Entities;

public sealed class ExportEntitiesQueryHandler(
    IEntityRepository entityRepository) : IQueryHandler<ExportEntitiesQuery, byte[]>
{
    private static readonly IReadOnlyList<ColumnDefinition<Entity>> Columns =
    [
        new("Id", e => e.Id),
        new("Nombre", e => e.Name),
        new("CUIT", e => e.Cuit),
        new("Tipo", e => e.Type.ToString()),
        new("Estado", e => e.Status.ToString()),
        new("Ciudad", e => e.City),
        new("Provincia", e => e.Province),
        new("Telefono", e => e.Phone),
        new("Email", e => e.Email),
        new("Creado", e => e.CreatedAt)
    ];

    public async ValueTask<byte[]> Handle(ExportEntitiesQuery query, CancellationToken ct)
    {
        var count = await entityRepository.CountForExportAsync(query.Search, query.Status, query.Type, ct);
        if (count > 10_000)
            throw new ValidationException("ORG_EXPORT_TOO_LARGE", "El límite de exportación es de 10.000 filas.");

        var entities = await entityRepository.ListForExportAsync(query.Search, query.Status, query.Type, ct);

        return query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase)
            ? ExportService.ToCsv(entities, Columns)
            : ExportService.ToXlsx(entities, Columns, "Entities");
    }
}
