using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class PlanAccountAttributesConfiguration : IEntityTypeConfiguration<PlanAccountAttributes>
{
    public void Configure(EntityTypeBuilder<PlanAccountAttributes> entity)
    {
        entity.ToTable("plan_account_attributes");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.MaintenanceFee).HasColumnName("maintenance_fee").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.MinimumBalance).HasColumnName("minimum_balance").HasPrecision(18, 2);
        entity.Property(x => x.InterestRate).HasColumnName("interest_rate").HasPrecision(18, 2);
        entity.Property(x => x.AccountType).HasColumnName("account_type").HasMaxLength(50).IsRequired();

        entity.HasIndex(x => x.PlanId).IsUnique();
    }
}
