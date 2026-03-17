using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class ObservationRepository(OperationsDbContext db) : IObservationRepository
{
    public async Task<IReadOnlyList<ApplicationObservation>> GetByApplicationIdAsync(
        Guid applicationId,
        CancellationToken ct = default)
    {
        return await db.ApplicationObservations
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ApplicationObservation observation, CancellationToken ct = default)
    {
        await db.ApplicationObservations.AddAsync(observation, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
