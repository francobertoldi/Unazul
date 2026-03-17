using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class ApplicationObservationConfiguration : IEntityTypeConfiguration<ApplicationObservation>
{
    public void Configure(EntityTypeBuilder<ApplicationObservation> entity)
    {
        entity.ToTable("application_observations");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ApplicationId).HasColumnName("application_id").IsRequired();
        entity.Property(x => x.ObservationType).HasColumnName("observation_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.Content).HasColumnName("content").IsRequired();
        entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        entity.HasIndex(x => new { x.ApplicationId, x.CreatedAt });

        entity.HasOne<Application>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
