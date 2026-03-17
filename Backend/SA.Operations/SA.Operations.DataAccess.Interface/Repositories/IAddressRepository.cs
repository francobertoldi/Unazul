using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IAddressRepository
{
    Task<IReadOnlyList<ApplicantAddress>> GetByApplicantIdAsync(Guid applicantId, CancellationToken ct = default);
    Task<ApplicantAddress?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ApplicantAddress address, CancellationToken ct = default);
    void Update(ApplicantAddress address);
    void Delete(ApplicantAddress address);
    Task SaveChangesAsync(CancellationToken ct = default);
}
