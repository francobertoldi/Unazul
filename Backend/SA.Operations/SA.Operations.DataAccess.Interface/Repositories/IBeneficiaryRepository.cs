using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IBeneficiaryRepository
{
    Task<IReadOnlyList<Beneficiary>> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
    Task AddAsync(Beneficiary beneficiary, CancellationToken ct = default);
    void Update(Beneficiary beneficiary);
    void Delete(Beneficiary beneficiary);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<decimal> SumPercentageAsync(Guid applicationId, CancellationToken ct = default);
}
