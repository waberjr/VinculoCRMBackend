using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorEmailConfiguration : IEntityTypeConfiguration<DonorEmail>
{
    public void Configure(EntityTypeBuilder<DonorEmail> builder)
    {
        builder.Property(e => e.Address).HasMaxLength(254).IsRequired();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId });
        builder.HasIndex(e => new { e.OrganizationId, e.Address });
        builder.HasOne(e => e.Donor)
            .WithMany(e => e.Emails)
            .HasForeignKey(e => e.DonorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
