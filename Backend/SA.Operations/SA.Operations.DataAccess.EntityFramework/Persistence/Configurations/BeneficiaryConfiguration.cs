using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> entity)
    {
        entity.ToTable("beneficiaries");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ApplicationId).HasColumnName("application_id").IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(150).IsRequired();
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(150).IsRequired();
        entity.Property(x => x.Relationship).HasColumnName("relationship").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Percentage).HasColumnName("percentage").HasPrecision(5, 2).IsRequired();

        entity.HasOne<Application>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
