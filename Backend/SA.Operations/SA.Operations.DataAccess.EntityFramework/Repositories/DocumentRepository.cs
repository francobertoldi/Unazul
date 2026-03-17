using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class DocumentRepository(OperationsDbContext db) : IDocumentRepository
{
    public async Task<IReadOnlyList<ApplicationDocument>> GetByApplicationIdAsync(
        Guid applicationId,
        CancellationToken ct = default)
    {
        return await db.ApplicationDocuments
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<ApplicationDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ApplicationDocuments.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(ApplicationDocument document, CancellationToken ct = default)
    {
        await db.ApplicationDocuments.AddAsync(document, ct);
    }

    public void Update(ApplicationDocument document)
    {
        db.ApplicationDocuments.Update(document);
    }

    public void Delete(ApplicationDocument document)
    {
        db.ApplicationDocuments.Remove(document);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
