using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ServiceAuthConfigConfiguration : IEntityTypeConfiguration<ServiceAuthConfig>
{
    public void Configure(EntityTypeBuilder<ServiceAuthConfig> entity)
    {
        entity.ToTable("service_auth_configs");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ServiceId).HasColumnName("service_id").IsRequired();
        entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.ValueEncrypted).HasColumnName("value_encrypted").HasMaxLength(4000).IsRequired();

        entity.HasIndex(x => new { x.ServiceId, x.Key }).IsUnique();
    }
}
