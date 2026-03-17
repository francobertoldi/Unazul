using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class PlanLoanAttributesConfiguration : IEntityTypeConfiguration<PlanLoanAttributes>
{
    public void Configure(EntityTypeBuilder<PlanLoanAttributes> entity)
    {
        entity.ToTable("plan_loan_attributes");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.AmortizationType).HasColumnName("amortization_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.AnnualEffectiveRate).HasColumnName("annual_effective_rate").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.CftRate).HasColumnName("cft_rate").HasPrecision(18, 2);
        entity.Property(x => x.AdminFees).HasColumnName("admin_fees").HasPrecision(18, 2);

        entity.HasIndex(x => x.PlanId).IsUnique();
    }
}
