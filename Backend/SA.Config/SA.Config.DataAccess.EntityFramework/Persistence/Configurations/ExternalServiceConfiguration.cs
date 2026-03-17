using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ExternalServiceConfiguration : IEntityTypeConfiguration<ExternalService>
{
    public void Configure(EntityTypeBuilder<ExternalService> entity)
    {
        entity.ToTable("external_services");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.BaseUrl).HasColumnName("base_url").HasMaxLength(2000).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.AuthType).HasColumnName("auth_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.TimeoutMs).HasColumnName("timeout_ms").IsRequired().HasDefaultValue(30000);
        entity.Property(x => x.MaxRetries).HasColumnName("max_retries").IsRequired().HasDefaultValue(3);
        entity.Property(x => x.LastTestedAt).HasColumnName("last_tested_at");
        entity.Property(x => x.LastTestSuccess).HasColumnName("last_test_success");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        entity.HasMany(x => x.AuthConfigs)
            .WithOne()
            .HasForeignKey(c => c.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
