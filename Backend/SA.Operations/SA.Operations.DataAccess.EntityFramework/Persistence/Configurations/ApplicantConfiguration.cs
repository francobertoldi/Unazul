using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> entity)
    {
        entity.ToTable("applicants");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        entity.Property(x => x.DocumentType).HasColumnName("document_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.DocumentNumber).HasColumnName("document_number").HasMaxLength(30).IsRequired();
        entity.Property(x => x.BirthDate).HasColumnName("birth_date");
        entity.Property(x => x.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Occupation).HasColumnName("occupation").HasMaxLength(200);

        entity.HasIndex(x => new { x.TenantId, x.DocumentType, x.DocumentNumber }).IsUnique();
    }
}
