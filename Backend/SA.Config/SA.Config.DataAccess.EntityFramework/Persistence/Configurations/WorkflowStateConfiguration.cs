using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class WorkflowStateConfiguration : IEntityTypeConfiguration<WorkflowState>
{
    public void Configure(EntityTypeBuilder<WorkflowState> entity)
    {
        entity.ToTable("workflow_states");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.WorkflowId).HasColumnName("workflow_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        entity.Property(x => x.Label).HasColumnName("label").HasMaxLength(200);
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.PositionX).HasColumnName("position_x").HasPrecision(18, 2);
        entity.Property(x => x.PositionY).HasColumnName("position_y").HasPrecision(18, 2);

        entity.HasMany(x => x.Configs)
            .WithOne()
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.Fields)
            .WithOne()
            .HasForeignKey(f => f.StateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
