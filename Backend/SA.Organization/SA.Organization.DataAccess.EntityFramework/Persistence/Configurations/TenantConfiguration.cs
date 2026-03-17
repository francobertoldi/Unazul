using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.ToTable("tenants");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(20).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(200);
        entity.Property(x => x.Province).HasColumnName("province").HasMaxLength(200);
        entity.Property(x => x.ZipCode).HasColumnName("zip_code").HasMaxLength(20);
        entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);
        entity.Property(x => x.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        entity.HasIndex(x => x.Identifier).IsUnique();
    }
}
