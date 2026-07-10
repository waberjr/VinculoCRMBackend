using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class CommunicationCampaignRecipientConfiguration : IEntityTypeConfiguration<CommunicationCampaignRecipient>
{
    public void Configure(EntityTypeBuilder<CommunicationCampaignRecipient> builder)
    {
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.BlockReason).HasMaxLength(500);

        builder.HasOne(entity => entity.CommunicationCampaign)
            .WithMany(entity => entity.Recipients)
            .HasForeignKey(entity => entity.CommunicationCampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Donor)
            .WithMany()
            .HasForeignKey(entity => entity.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.TimelineEntry)
            .WithMany()
            .HasForeignKey(entity => entity.TimelineEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.CommunicationCampaignId, entity.DonorId })
            .IsUnique();
    }
}
