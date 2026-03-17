using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.Interface.Repositories;

public interface IParameterRepository
{
    Task<IReadOnlyList<Parameter>> GetByGroupIdAsync(Guid groupId, string? parentKey = null, CancellationToken ct = default);
    Task<Parameter?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByKeyAsync(Guid tenantId, Guid groupId, string key, CancellationToken ct = default);
    Task AddAsync(Parameter parameter, CancellationToken ct = default);
    void Update(Parameter parameter);
    Task DeleteAsync(Parameter parameter, CancellationToken ct = default); // deletes options too
    Task ReplaceOptionsAsync(Guid parameterId, IEnumerable<ParameterOption> newOptions, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
