using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.Property(entity => entity.Number).HasMaxLength(40);
        builder.Property(entity => entity.Amount).HasPrecision(12, 2);
        builder.Property(entity => entity.FileUrl).HasMaxLength(500);
        builder.Property(entity => entity.CancelReason).HasMaxLength(500);
        builder.Property(entity => entity.IssuedByUserId).HasMaxLength(450);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.DonationId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Number })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(entity => new { entity.OrganizationId, entity.DonorId, entity.IssuedAtUtc });

        builder.HasOne(entity => entity.Donation)
            .WithMany()
            .HasForeignKey(entity => entity.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Donor)
            .WithMany()
            .HasForeignKey(entity => entity.DonorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
