using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class CommissionPlanConfiguration : IEntityTypeConfiguration<CommissionPlan>
{
    public void Configure(EntityTypeBuilder<CommissionPlan> entity)
    {
        entity.ToTable("commission_plans");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Value).HasColumnName("value").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.MaxAmount).HasColumnName("max_amount").HasPrecision(18, 2);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
