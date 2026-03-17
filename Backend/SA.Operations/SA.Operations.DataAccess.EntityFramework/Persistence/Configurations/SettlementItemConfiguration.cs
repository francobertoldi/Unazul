using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class SettlementItemConfiguration : IEntityTypeConfiguration<SettlementItem>
{
    public void Configure(EntityTypeBuilder<SettlementItem> entity)
    {
        entity.ToTable("settlement_items");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.SettlementId).HasColumnName("settlement_id").IsRequired();
        entity.Property(x => x.ApplicationId).HasColumnName("application_id").IsRequired();
        entity.Property(x => x.CommissionType).HasColumnName("commission_type").HasMaxLength(50).IsRequired();
        entity.Property(x => x.CommissionValue).HasColumnName("commission_value").HasPrecision(18, 4).IsRequired();
        entity.Property(x => x.CalculatedAmount).HasColumnName("calculated_amount").HasPrecision(18, 4).IsRequired();
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        entity.Property(x => x.FormulaDescription).HasColumnName("formula_description").HasMaxLength(500).IsRequired();

        entity.HasIndex(x => x.SettlementId);
        entity.HasIndex(x => x.ApplicationId);
    }
}
