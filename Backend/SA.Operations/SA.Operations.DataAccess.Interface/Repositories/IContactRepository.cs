using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IContactRepository
{
    Task<IReadOnlyList<ApplicantContact>> GetByApplicantIdAsync(Guid applicantId, CancellationToken ct = default);
    Task<ApplicantContact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ApplicantContact contact, CancellationToken ct = default);
    void Update(ApplicantContact contact);
    void Delete(ApplicantContact contact);
    Task SaveChangesAsync(CancellationToken ct = default);
}
