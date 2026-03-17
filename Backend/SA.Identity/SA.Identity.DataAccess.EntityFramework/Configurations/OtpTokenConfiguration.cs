using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Configurations;

public sealed class OtpTokenConfiguration : IEntityTypeConfiguration<OtpToken>
{
    public void Configure(EntityTypeBuilder<OtpToken> entity)
    {
        entity.ToTable("otp_tokens");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(x => x.CodeHash).HasColumnName("code_hash").HasMaxLength(128).IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        entity.Property(x => x.Used).HasColumnName("used").HasDefaultValue(false);
        entity.Property(x => x.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
        entity.Property(x => x.ResendCount).HasColumnName("resend_count").HasDefaultValue(0);

        entity.HasIndex(x => new { x.UserId, x.Used });
    }
}
