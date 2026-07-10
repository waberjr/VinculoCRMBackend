using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class CommunicationCampaignConfiguration : IEntityTypeConfiguration<CommunicationCampaign>
{
    public void Configure(EntityTypeBuilder<CommunicationCampaign> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Channel).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Audience).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);

        builder.HasOne(entity => entity.Template)
            .WithMany()
            .HasForeignKey(entity => entity.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.Status, entity.PlannedAtUtc });
    }
}
