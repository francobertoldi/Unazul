using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IObservationRepository
{
    Task<IReadOnlyList<ApplicationObservation>> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
    Task AddAsync(ApplicationObservation observation, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
