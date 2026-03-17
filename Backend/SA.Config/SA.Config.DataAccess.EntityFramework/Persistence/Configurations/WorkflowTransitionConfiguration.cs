using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> entity)
    {
        entity.ToTable("workflow_transitions");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.WorkflowId).HasColumnName("workflow_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.FromStateId).HasColumnName("from_state_id").IsRequired();
        entity.Property(x => x.ToStateId).HasColumnName("to_state_id").IsRequired();
        entity.Property(x => x.Label).HasColumnName("label").HasMaxLength(200);
        entity.Property(x => x.Condition).HasColumnName("condition").HasMaxLength(500);
        entity.Property(x => x.SlaHours).HasColumnName("sla_hours");

        entity.HasOne<WorkflowState>()
            .WithMany()
            .HasForeignKey(x => x.FromStateId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<WorkflowState>()
            .WithMany()
            .HasForeignKey(x => x.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
