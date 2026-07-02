using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorTagAssignmentConfiguration : IEntityTypeConfiguration<DonorTagAssignment>
{
    public void Configure(EntityTypeBuilder<DonorTagAssignment> builder)
    {
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.DonorTagId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasOne(e => e.Donor)
            .WithMany(e => e.TagAssignments)
            .HasForeignKey(e => e.DonorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.DonorTag)
            .WithMany()
            .HasForeignKey(e => e.DonorTagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
