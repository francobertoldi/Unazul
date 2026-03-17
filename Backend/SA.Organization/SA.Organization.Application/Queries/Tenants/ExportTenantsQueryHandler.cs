using Mediator;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Exceptions;
using Shared.Export;

namespace SA.Organization.Application.Queries.Tenants;

public sealed class ExportTenantsQueryHandler(
    ITenantRepository tenantRepository) : IQueryHandler<ExportTenantsQuery, byte[]>
{
    private static readonly IReadOnlyList<ColumnDefinition<Tenant>> Columns =
    [
        new("Id", t => t.Id),
        new("Nombre", t => t.Name),
        new("Identificador", t => t.Identifier),
        new("Estado", t => t.Status.ToString()),
        new("Ciudad", t => t.City),
        new("Provincia", t => t.Province),
        new("Telefono", t => t.Phone),
        new("Email", t => t.Email),
        new("Creado", t => t.CreatedAt)
    ];

    public async ValueTask<byte[]> Handle(ExportTenantsQuery query, CancellationToken ct)
    {
        var count = await tenantRepository.CountForExportAsync(query.Search, query.Status, ct);
        if (count > 10_000)
            throw new ValidationException("ORG_EXPORT_TOO_LARGE", "El límite de exportación es de 10.000 filas.");

        var tenants = await tenantRepository.ListForExportAsync(query.Search, query.Status, ct);

        return query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase)
            ? ExportService.ToCsv(tenants, Columns)
            : ExportService.ToXlsx(tenants, Columns, "Tenants");
    }
}
