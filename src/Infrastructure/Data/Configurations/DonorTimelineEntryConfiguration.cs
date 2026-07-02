using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorTimelineEntryConfiguration : IEntityTypeConfiguration<DonorTimelineEntry>
{
    public void Configure(EntityTypeBuilder<DonorTimelineEntry> builder)
    {
        builder.Property(e => e.Title).HasMaxLength(180).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.CreatedByUserId).HasMaxLength(450);
        builder.Property(e => e.RelatedEntityType).HasMaxLength(80);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.OccurredAtUtc }).IsDescending(false, false, true);
    }
}
