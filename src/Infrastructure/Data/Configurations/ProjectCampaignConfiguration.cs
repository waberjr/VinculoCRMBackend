using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class ProjectCampaignConfiguration : IEntityTypeConfiguration<ProjectCampaign>
{
    public void Configure(EntityTypeBuilder<ProjectCampaign> builder)
    {
        builder.HasIndex(e => new { e.OrganizationId, e.ProjectId, e.CampaignId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Campaign)
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
