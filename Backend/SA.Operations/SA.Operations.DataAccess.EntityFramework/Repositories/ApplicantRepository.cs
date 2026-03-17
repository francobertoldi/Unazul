using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class ApplicantRepository(OperationsDbContext db) : IApplicantRepository
{
    public async Task<Applicant?> GetByDocumentAsync(
        Guid tenantId,
        DocumentType docType,
        string docNumber,
        CancellationToken ct = default)
    {
        return await db.Applicants
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.DocumentType == docType &&
                x.DocumentNumber == docNumber, ct);
    }

    public async Task<Applicant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Applicants.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, Applicant>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        var applicants = await db.Applicants
            .AsNoTracking()
            .Where(a => idList.Contains(a.Id))
            .ToListAsync(ct);
        return applicants.ToDictionary(a => a.Id);
    }

    public async Task<Applicant?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Applicants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(Applicant applicant, CancellationToken ct = default)
    {
        await db.Applicants.AddAsync(applicant, ct);
    }

    public void Update(Applicant applicant)
    {
        db.Applicants.Update(applicant);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> CountApplicationsAsync(Guid applicantId, CancellationToken ct = default)
    {
        return await db.Applications.CountAsync(x => x.ApplicantId == applicantId, ct);
    }
}
