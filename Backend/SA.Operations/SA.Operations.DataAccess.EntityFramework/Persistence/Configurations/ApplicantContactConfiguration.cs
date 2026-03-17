using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ApplicantContactConfiguration : IEntityTypeConfiguration<ApplicantContact>
{
    public void Configure(EntityTypeBuilder<ApplicantContact> entity)
    {
        entity.ToTable("applicant_contacts");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ApplicantId).HasColumnName("applicant_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
        entity.Property(x => x.PhoneCode).HasColumnName("phone_code").HasMaxLength(10);
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);

        entity.HasIndex(x => x.ApplicantId);

        entity.HasOne<Applicant>()
            .WithMany()
            .HasForeignKey(x => x.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
