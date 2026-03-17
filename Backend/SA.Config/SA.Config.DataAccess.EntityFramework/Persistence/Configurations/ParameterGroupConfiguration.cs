using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ParameterGroupConfiguration : IEntityTypeConfiguration<ParameterGroup>
{
    public void Configure(EntityTypeBuilder<ParameterGroup> entity)
    {
        entity.ToTable("parameter_groups");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Icon).HasColumnName("icon").HasMaxLength(100).IsRequired();
        entity.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        entity.HasIndex(x => x.Code).IsUnique();
    }
}
