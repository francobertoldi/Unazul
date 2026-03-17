using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ParameterOptionConfiguration : IEntityTypeConfiguration<ParameterOption>
{
    public void Configure(EntityTypeBuilder<ParameterOption> entity)
    {
        entity.ToTable("parameter_options");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ParameterId).HasColumnName("parameter_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.OptionValue).HasColumnName("option_value").HasMaxLength(500).IsRequired();
        entity.Property(x => x.OptionLabel).HasColumnName("option_label").HasMaxLength(500).IsRequired();
        entity.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();
    }
}
