using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence.Configurations;

public sealed class TraceEventDetailConfiguration : IEntityTypeConfiguration<TraceEventDetail>
{
    public void Configure(EntityTypeBuilder<TraceEventDetail> entity)
    {
        entity.ToTable("trace_event_details");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.TraceEventId).HasColumnName("trace_event_id").IsRequired();
        entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Value).HasColumnName("value").HasMaxLength(2000).IsRequired();

        entity.HasIndex(x => x.TraceEventId);
    }
}
