using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ApplicantAddressConfiguration : IEntityTypeConfiguration<ApplicantAddress>
{
    public void Configure(EntityTypeBuilder<ApplicantAddress> entity)
    {
        entity.ToTable("applicant_addresses");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ApplicantId).HasColumnName("applicant_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Street).HasColumnName("street").HasMaxLength(300).IsRequired();
        entity.Property(x => x.Number).HasColumnName("number").HasMaxLength(20).IsRequired();
        entity.Property(x => x.Floor).HasColumnName("floor").HasMaxLength(10);
        entity.Property(x => x.Apartment).HasColumnName("apartment").HasMaxLength(10);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Province).HasColumnName("province").HasMaxLength(100).IsRequired();
        entity.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20).IsRequired();
        entity.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(10, 7);
        entity.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(10, 7);

        entity.HasIndex(x => x.ApplicantId);

        entity.HasOne<Applicant>()
            .WithMany()
            .HasForeignKey(x => x.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
