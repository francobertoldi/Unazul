using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Application?> GetByIdWithApplicantAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Application> Items, int Total)> ListAsync(
        int skip,
        int take,
        string? search = null,
        ApplicationStatus? status = null,
        Guid? entityId = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Application application, CancellationToken ct = default);
    void Update(Application application);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetNextSequenceAsync(Guid tenantId, CancellationToken ct = default);
    Task<int> TransitionStatusAsync(Guid id, ApplicationStatus currentStatus, ApplicationStatus newStatus, Guid updatedBy, CancellationToken ct = default);
    Task<IReadOnlyList<Application>> GetApprovedByDateRangeAsync(
        Guid tenantId,
        Guid? entityId,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default);
    Task<int> BatchTransitionToSettledAsync(IReadOnlyList<Guid> ids, Guid updatedBy, CancellationToken ct = default);
}
