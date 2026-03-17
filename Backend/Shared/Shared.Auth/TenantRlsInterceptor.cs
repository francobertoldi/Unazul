using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Shared.Auth;

public sealed class TenantRlsInterceptor(ICurrentUser currentUser) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetTenant(connection);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await SetTenantAsync(connection, cancellationToken);
    }

    private void SetTenant(DbConnection connection)
    {
        if (currentUser.TenantId == Guid.Empty) return;

        // Guid.ToString("D") guarantees format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        // (only hex digits and dashes) — safe from SQL injection by construction.
        var tenantId = currentUser.TenantId.ToString("D");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SET LOCAL app.current_tenant = '{tenantId}'";
        cmd.ExecuteNonQuery();
    }

    private async Task SetTenantAsync(DbConnection connection, CancellationToken ct)
    {
        if (currentUser.TenantId == Guid.Empty) return;

        var tenantId = currentUser.TenantId.ToString("D");

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SET LOCAL app.current_tenant = '{tenantId}'";
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
