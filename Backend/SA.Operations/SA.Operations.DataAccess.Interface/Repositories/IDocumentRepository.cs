using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface IDocumentRepository
{
    Task<IReadOnlyList<ApplicationDocument>> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
    Task<ApplicationDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ApplicationDocument document, CancellationToken ct = default);
    void Update(ApplicationDocument document);
    void Delete(ApplicationDocument document);
    Task SaveChangesAsync(CancellationToken ct = default);
}
