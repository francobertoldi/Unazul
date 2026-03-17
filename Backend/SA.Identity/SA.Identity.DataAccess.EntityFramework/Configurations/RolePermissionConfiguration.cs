using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("role_permissions");
        entity.HasKey(x => new { x.RoleId, x.PermissionId });

        entity.Property(x => x.RoleId).HasColumnName("role_id");
        entity.Property(x => x.PermissionId).HasColumnName("permission_id");
    }
}
