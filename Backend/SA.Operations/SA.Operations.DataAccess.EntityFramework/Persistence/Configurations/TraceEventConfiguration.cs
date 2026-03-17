using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class TraceEventConfiguration : IEntityTypeConfiguration<TraceEvent>
{
    public void Configure(EntityTypeBuilder<TraceEvent> entity)
    {
        entity.ToTable("trace_events");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.ApplicationId).HasColumnName("application_id").IsRequired();
        entity.Property(x => x.State).HasColumnName("state").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(200).IsRequired();
        entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();

        entity.HasIndex(x => new { x.ApplicationId, x.OccurredAt });

        entity.HasOne<Application>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Details)
            .WithOne()
            .HasForeignKey(d => d.TraceEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
