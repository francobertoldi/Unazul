using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class WorkflowStateFieldConfiguration : IEntityTypeConfiguration<WorkflowStateField>
{
    public void Configure(EntityTypeBuilder<WorkflowStateField> entity)
    {
        entity.ToTable("workflow_state_fields");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.StateId).HasColumnName("state_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.FieldName).HasColumnName("field_name").HasMaxLength(100);
        entity.Property(x => x.FieldType).HasColumnName("field_type").HasMaxLength(50);
        entity.Property(x => x.IsRequired).HasColumnName("is_required");
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
    }
}
