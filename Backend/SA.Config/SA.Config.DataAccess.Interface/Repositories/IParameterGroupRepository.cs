using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.Interface.Repositories;

public interface IParameterGroupRepository
{
    Task<IReadOnlyList<ParameterGroup>> GetAllOrderedAsync(CancellationToken ct = default);
    Task<ParameterGroup?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ParameterGroup?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> HasParametersAsync(Guid groupId, CancellationToken ct = default); // Check ANY tenant
    Task AddAsync(ParameterGroup group, CancellationToken ct = default);
    Task DeleteAsync(ParameterGroup group, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
