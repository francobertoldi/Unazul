using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> entity)
    {
        entity.ToTable("notification_templates");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(20).IsRequired();
        entity.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(200);
        entity.Property(x => x.Body).HasColumnName("body").HasMaxLength(4000).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("active");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
