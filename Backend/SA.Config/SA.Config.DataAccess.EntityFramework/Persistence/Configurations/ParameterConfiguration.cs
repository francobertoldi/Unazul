using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> entity)
    {
        entity.ToTable("parameters");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.GroupId).HasColumnName("group_id").IsRequired();
        entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Value).HasColumnName("value").HasMaxLength(4000).IsRequired();
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        entity.Property(x => x.ParentKey).HasColumnName("parent_key").HasMaxLength(100);
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.GroupId, x.Key }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.ParentKey });

        entity.HasOne<ParameterGroup>()
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(o => o.ParameterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
