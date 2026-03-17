using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class WorkflowStateConfigConfiguration : IEntityTypeConfiguration<WorkflowStateConfig>
{
    public void Configure(EntityTypeBuilder<WorkflowStateConfig> entity)
    {
        entity.ToTable("workflow_state_configs");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.StateId).HasColumnName("state_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(200);
        entity.Property(x => x.Value).HasColumnName("value").HasMaxLength(4000);

        entity.HasIndex(x => new { x.StateId, x.Key }).IsUnique();
    }
}
