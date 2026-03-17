using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> entity)
    {
        entity.ToTable("applications");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired();
        entity.Property(x => x.ApplicantId).HasColumnName("applicant_id").IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.PlanName).HasColumnName("plan_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.WorkflowStage).HasColumnName("workflow_stage").HasMaxLength(100);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.Status });
        entity.HasIndex(x => new { x.TenantId, x.EntityId });
        entity.HasIndex(x => new { x.TenantId, x.ApplicantId });

        entity.HasOne<Applicant>()
            .WithMany()
            .HasForeignKey(x => x.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
