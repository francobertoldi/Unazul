using Microsoft.EntityFrameworkCore;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Persistence;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EntityChannel> EntityChannels => Set<EntityChannel>();
    public DbSet<Branch> Branches => Set<Branch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
    }
}
