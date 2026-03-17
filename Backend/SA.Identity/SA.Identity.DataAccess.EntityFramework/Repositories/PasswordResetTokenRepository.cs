using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Repositories;

public sealed class PasswordResetTokenRepository(IdentityDbContext db) : IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await db.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await db.PasswordResetTokens.AddAsync(token, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        db.PasswordResetTokens.Update(token);
        await db.SaveChangesAsync(ct);
    }
}
