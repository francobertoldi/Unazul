using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Audit.Domain.Entities;

namespace SA.Audit.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("audit_log");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Operation).HasColumnName("operation").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Module).HasColumnName("module").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(500).IsRequired();
        entity.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(4000);
        entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired();
        entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(100);
        entity.Property(x => x.EntityId).HasColumnName("entity_id");
        entity.Property(x => x.ChangesJson).HasColumnName("changes_json").HasMaxLength(8000);
        entity.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.OccurredAt })
            .IsDescending(false, true);

        entity.HasIndex(x => new { x.TenantId, x.UserId, x.OccurredAt })
            .IsDescending(false, false, true);

        entity.HasIndex(x => new { x.TenantId, x.Module, x.OccurredAt })
            .IsDescending(false, false, true);

        entity.HasIndex(x => new { x.TenantId, x.Operation, x.OccurredAt })
            .IsDescending(false, false, true);
    }
}
