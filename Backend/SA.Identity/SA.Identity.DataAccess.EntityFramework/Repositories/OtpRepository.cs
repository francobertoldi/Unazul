using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Repositories;

public sealed class OtpRepository(IdentityDbContext db) : IOtpRepository
{
    public async Task<OtpToken?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.OtpTokens
            .Where(t => t.UserId == userId && !t.Used)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(OtpToken token, CancellationToken ct = default)
    {
        await db.OtpTokens.AddAsync(token, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(OtpToken token, CancellationToken ct = default)
    {
        db.OtpTokens.Update(token);
        await db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await db.OtpTokens
            .Where(t => t.UserId == userId && !t.Used)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }

        await db.SaveChangesAsync(ct);
    }
}
