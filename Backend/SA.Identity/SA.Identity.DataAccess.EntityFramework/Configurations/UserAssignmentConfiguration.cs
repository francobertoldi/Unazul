using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Configurations;

public sealed class UserAssignmentConfiguration : IEntityTypeConfiguration<UserAssignment>
{
    public void Configure(EntityTypeBuilder<UserAssignment> entity)
    {
        entity.ToTable("user_assignments");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(50).IsRequired();
        entity.Property(x => x.ScopeId).HasColumnName("scope_id").IsRequired();
        entity.Property(x => x.ScopeName).HasColumnName("scope_name").HasMaxLength(200).IsRequired();

        entity.HasIndex(x => new { x.UserId, x.ScopeType, x.ScopeId }).IsUnique();
    }
}
