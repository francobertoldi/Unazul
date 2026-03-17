using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    public void Configure(EntityTypeBuilder<Coverage> entity)
    {
        entity.ToTable("coverages");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.CoverageType).HasColumnName("coverage_type").HasMaxLength(100).IsRequired();
        entity.Property(x => x.SumInsured).HasColumnName("sum_insured").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.Premium).HasColumnName("premium").HasPrecision(18, 2);
        entity.Property(x => x.GracePeriodDays).HasColumnName("grace_period_days");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => x.PlanId);
        entity.HasIndex(x => new { x.PlanId, x.Name }).IsUnique();
    }
}
