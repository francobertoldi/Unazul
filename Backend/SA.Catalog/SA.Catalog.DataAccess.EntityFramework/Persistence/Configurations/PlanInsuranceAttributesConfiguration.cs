using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class PlanInsuranceAttributesConfiguration : IEntityTypeConfiguration<PlanInsuranceAttributes>
{
    public void Configure(EntityTypeBuilder<PlanInsuranceAttributes> entity)
    {
        entity.ToTable("plan_insurance_attributes");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.Premium).HasColumnName("premium").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.SumInsured).HasColumnName("sum_insured").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.GracePeriodDays).HasColumnName("grace_period_days");
        entity.Property(x => x.CoverageType).HasColumnName("coverage_type").HasMaxLength(100).IsRequired();

        entity.HasIndex(x => x.PlanId).IsUnique();
    }
}
