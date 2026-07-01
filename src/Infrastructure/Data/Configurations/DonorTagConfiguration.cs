using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorTagConfiguration : IEntityTypeConfiguration<DonorTag>
{
    public void Configure(EntityTypeBuilder<DonorTag> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(300);
        builder.HasIndex(e => new { e.OrganizationId, e.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
