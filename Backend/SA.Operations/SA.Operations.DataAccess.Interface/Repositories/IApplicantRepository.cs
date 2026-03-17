using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IApplicantRepository
{
    Task<Applicant?> GetByDocumentAsync(Guid tenantId, DocumentType docType, string docNumber, CancellationToken ct = default);
    Task<Applicant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, Applicant>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Applicant?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Applicant applicant, CancellationToken ct = default);
    void Update(Applicant applicant);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> CountApplicationsAsync(Guid applicantId, CancellationToken ct = default);
}
