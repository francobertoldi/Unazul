using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class TraceEventRepository(OperationsDbContext db) : ITraceEventRepository
{
    public async Task<IReadOnlyList<TraceEvent>> GetByApplicationIdAsync(
        Guid applicationId,
        CancellationToken ct = default)
    {
        return await db.TraceEvents
            .AsNoTracking()
            .Include(x => x.Details)
            .Where(x => x.ApplicationId == applicationId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TraceEvent traceEvent, CancellationToken ct = default)
    {
        await db.TraceEvents.AddAsync(traceEvent, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
