using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Seed;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAssignment> UserAssignments => Set<UserAssignment>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpToken> OtpTokens => Set<OtpToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Seed 88 atomic permissions (global catalog, no tenant_id). RN-SEC-17.
        modelBuilder.Entity<Permission>().HasData(
            PermissionSeedData.GetSeedObjects());

    }
}
