using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class BeneficiaryRepository(OperationsDbContext db) : IBeneficiaryRepository
{
    public async Task<IReadOnlyList<Beneficiary>> GetByApplicationIdAsync(
        Guid applicationId,
        CancellationToken ct = default)
    {
        return await db.Beneficiaries
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Beneficiary beneficiary, CancellationToken ct = default)
    {
        await db.Beneficiaries.AddAsync(beneficiary, ct);
    }

    public void Update(Beneficiary beneficiary)
    {
        db.Beneficiaries.Update(beneficiary);
    }

    public void Delete(Beneficiary beneficiary)
    {
        db.Beneficiaries.Remove(beneficiary);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task<decimal> SumPercentageAsync(Guid applicationId, CancellationToken ct = default)
    {
        return await db.Beneficiaries
            .Where(x => x.ApplicationId == applicationId)
            .SumAsync(x => x.Percentage, ct);
    }
}
