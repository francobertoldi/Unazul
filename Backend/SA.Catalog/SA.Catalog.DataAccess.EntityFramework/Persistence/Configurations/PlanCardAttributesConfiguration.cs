using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class PlanCardAttributesConfiguration : IEntityTypeConfiguration<PlanCardAttributes>
{
    public void Configure(EntityTypeBuilder<PlanCardAttributes> entity)
    {
        entity.ToTable("plan_card_attributes");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.CreditLimit).HasColumnName("credit_limit").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.AnnualFee).HasColumnName("annual_fee").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.InterestRate).HasColumnName("interest_rate").HasPrecision(18, 2);
        entity.Property(x => x.Network).HasColumnName("network").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Level).HasColumnName("level").HasMaxLength(50).IsRequired();

        entity.HasIndex(x => x.PlanId).IsUnique();
    }
}
