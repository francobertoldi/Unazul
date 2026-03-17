using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Seed;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence;

public sealed class ConfigDbContext(DbContextOptions<ConfigDbContext> options) : DbContext(options)
{
    public DbSet<ParameterGroup> ParameterGroups => Set<ParameterGroup>();
    public DbSet<Parameter> Parameters => Set<Parameter>();
    public DbSet<ParameterOption> ParameterOptions => Set<ParameterOption>();
    public DbSet<ExternalService> ExternalServices => Set<ExternalService>();
    public DbSet<ServiceAuthConfig> ServiceAuthConfigs => Set<ServiceAuthConfig>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowStateConfig> WorkflowStateConfigs => Set<WorkflowStateConfig>();
    public DbSet<WorkflowStateField> WorkflowStateFields => Set<WorkflowStateField>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigDbContext).Assembly);

        // Seed 14 parameter groups (global catalog, no tenant_id).
        modelBuilder.Entity<ParameterGroup>().HasData(
            ParameterGroupSeedData.GetSeedObjects());
    }
}
