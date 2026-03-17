using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.ToTable("branches");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        entity.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(200);
        entity.Property(x => x.Province).HasColumnName("province").HasMaxLength(200);
        entity.Property(x => x.ZipCode).HasColumnName("zip_code").HasMaxLength(20);
        entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);
        entity.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
