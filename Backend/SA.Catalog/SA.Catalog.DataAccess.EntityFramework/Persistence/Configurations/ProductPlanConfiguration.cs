using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ProductPlanConfiguration : IEntityTypeConfiguration<ProductPlan>
{
    public void Configure(EntityTypeBuilder<ProductPlan> entity)
    {
        entity.ToTable("product_plans");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2).IsRequired();
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.Installments).HasColumnName("installments");
        entity.Property(x => x.CommissionPlanId).HasColumnName("commission_plan_id");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => x.ProductId);

        entity.HasOne(x => x.CommissionPlan)
            .WithMany()
            .HasForeignKey(x => x.CommissionPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(x => x.LoanAttributes)
            .WithOne()
            .HasForeignKey<PlanLoanAttributes>(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.InsuranceAttributes)
            .WithOne()
            .HasForeignKey<PlanInsuranceAttributes>(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.AccountAttributes)
            .WithOne()
            .HasForeignKey<PlanAccountAttributes>(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.CardAttributes)
            .WithOne()
            .HasForeignKey<PlanCardAttributes>(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.InvestmentAttributes)
            .WithOne()
            .HasForeignKey<PlanInvestmentAttributes>(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.Coverages)
            .WithOne()
            .HasForeignKey(c => c.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
