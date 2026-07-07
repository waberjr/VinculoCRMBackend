using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonationProjectConfiguration : IEntityTypeConfiguration<DonationProject>
{
    public void Configure(EntityTypeBuilder<DonationProject> builder)
    {
        builder.HasIndex(e => new { e.OrganizationId, e.DonationId, e.ProjectId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(e => new { e.OrganizationId, e.ProjectId });
        builder.HasOne(e => e.Donation)
            .WithMany()
            .HasForeignKey(e => e.DonationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
