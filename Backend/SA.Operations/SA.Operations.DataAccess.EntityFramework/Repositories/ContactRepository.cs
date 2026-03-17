using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class ContactRepository(OperationsDbContext db) : IContactRepository
{
    public async Task<IReadOnlyList<ApplicantContact>> GetByApplicantIdAsync(
        Guid applicantId,
        CancellationToken ct = default)
    {
        return await db.ApplicantContacts
            .AsNoTracking()
            .Where(x => x.ApplicantId == applicantId)
            .OrderBy(x => x.Type)
            .ToListAsync(ct);
    }

    public async Task<ApplicantContact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ApplicantContacts.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(ApplicantContact contact, CancellationToken ct = default)
    {
        await db.ApplicantContacts.AddAsync(contact, ct);
    }

    public void Update(ApplicantContact contact)
    {
        db.ApplicantContacts.Update(contact);
    }

    public void Delete(ApplicantContact contact)
    {
        db.ApplicantContacts.Remove(contact);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
