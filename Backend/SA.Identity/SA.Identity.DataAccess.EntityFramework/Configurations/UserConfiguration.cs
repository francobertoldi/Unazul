using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(30).IsRequired();
        entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.EntityId).HasColumnName("entity_id");
        entity.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(200);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
        entity.Property(x => x.LastLogin).HasColumnName("last_login");
        entity.Property(x => x.Avatar).HasColumnName("avatar").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

        entity.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.EntityId });
        entity.HasIndex(x => new { x.TenantId, x.Status });

        entity.HasMany(x => x.Assignments)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
