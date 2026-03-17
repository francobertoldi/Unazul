using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permissions");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Module).HasColumnName("module").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);

        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
