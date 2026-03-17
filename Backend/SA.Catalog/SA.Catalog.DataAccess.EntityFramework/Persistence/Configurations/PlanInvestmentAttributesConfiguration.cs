using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class PlanInvestmentAttributesConfiguration : IEntityTypeConfiguration<PlanInvestmentAttributes>
{
    public void Configure(EntityTypeBuilder<PlanInvestmentAttributes> entity)
    {
        entity.ToTable("plan_investment_attributes");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.MinimumAmount).HasColumnName("minimum_amount").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.ExpectedReturn).HasColumnName("expected_return").HasPrecision(18, 2);
        entity.Property(x => x.TermDays).HasColumnName("term_days");
        entity.Property(x => x.RiskLevel).HasColumnName("risk_level").HasConversion<string>().HasMaxLength(30).IsRequired();

        entity.HasIndex(x => x.PlanId).IsUnique();
    }
}
