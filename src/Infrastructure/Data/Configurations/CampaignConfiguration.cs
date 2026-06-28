using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(180).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.GoalAmount).HasPrecision(12, 2);
        builder.Property(e => e.AssignedUserId).HasMaxLength(450);
        builder.HasIndex(e => new { e.OrganizationId, e.StatusOptionId });
        builder.HasIndex(e => new { e.OrganizationId, e.Name });
    }
}
