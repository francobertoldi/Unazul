using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class EntityChannelConfiguration : IEntityTypeConfiguration<EntityChannel>
{
    public void Configure(EntityTypeBuilder<EntityChannel> entity)
    {
        entity.ToTable("entity_channels");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.ChannelType).HasColumnName("channel_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
    }
}
