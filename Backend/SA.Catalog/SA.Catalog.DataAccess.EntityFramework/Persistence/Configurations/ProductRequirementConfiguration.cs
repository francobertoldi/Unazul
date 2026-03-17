using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ProductRequirementConfiguration : IEntityTypeConfiguration<ProductRequirement>
{
    public void Configure(EntityTypeBuilder<ProductRequirement> entity)
    {
        entity.ToTable("product_requirements");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
        entity.Property(x => x.IsMandatory).HasColumnName("is_mandatory").IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => x.ProductId);
    }
}
