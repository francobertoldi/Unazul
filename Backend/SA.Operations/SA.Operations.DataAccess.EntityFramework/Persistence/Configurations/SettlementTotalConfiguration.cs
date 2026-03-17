using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class SettlementTotalConfiguration : IEntityTypeConfiguration<SettlementTotal>
{
    public void Configure(EntityTypeBuilder<SettlementTotal> entity)
    {
        entity.ToTable("settlement_totals");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.SettlementId).HasColumnName("settlement_id").IsRequired();
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 4).IsRequired();

        entity.HasIndex(x => x.SettlementId);
    }
}
