using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class AddressRepository(OperationsDbContext db) : IAddressRepository
{
    public async Task<IReadOnlyList<ApplicantAddress>> GetByApplicantIdAsync(
        Guid applicantId,
        CancellationToken ct = default)
    {
        return await db.ApplicantAddresses
            .AsNoTracking()
            .Where(x => x.ApplicantId == applicantId)
            .OrderBy(x => x.Type)
            .ToListAsync(ct);
    }

    public async Task<ApplicantAddress?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ApplicantAddresses.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(ApplicantAddress address, CancellationToken ct = default)
    {
        await db.ApplicantAddresses.AddAsync(address, ct);
    }

    public void Update(ApplicantAddress address)
    {
        db.ApplicantAddresses.Update(address);
    }

    public void Delete(ApplicantAddress address)
    {
        db.ApplicantAddresses.Remove(address);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
