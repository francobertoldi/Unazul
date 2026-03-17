using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> entity)
    {
        entity.ToTable("settlements");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.SettledAt).HasColumnName("settled_at").IsRequired();
        entity.Property(x => x.SettledBy).HasColumnName("settled_by").IsRequired();
        entity.Property(x => x.SettledByName).HasColumnName("settled_by_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.OperationCount).HasColumnName("operation_count").IsRequired();
        entity.Property(x => x.ExcelUrl).HasColumnName("excel_url").HasMaxLength(500);

        entity.HasIndex(x => new { x.TenantId, x.SettledAt }).IsDescending(false, true);

        entity.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.Totals)
            .WithOne()
            .HasForeignKey(t => t.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
