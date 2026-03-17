using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface ITraceEventRepository
{
    Task<IReadOnlyList<TraceEvent>> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
    Task AddAsync(TraceEvent traceEvent, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
