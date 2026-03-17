using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.Interface.Repositories;

public interface IOtpRepository
{
    Task<OtpToken?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(OtpToken token, CancellationToken ct = default);
    Task UpdateAsync(OtpToken token, CancellationToken ct = default);
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default);
}
