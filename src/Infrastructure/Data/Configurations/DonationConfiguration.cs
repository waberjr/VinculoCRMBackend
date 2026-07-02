using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.Property(e => e.Amount).HasPrecision(12, 2);
        builder.Property(e => e.Reference).HasMaxLength(120);
        builder.Property(e => e.ExternalPaymentId).HasMaxLength(160);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.CancellationReason).HasMaxLength(500);
        builder.Property(e => e.RefundReason).HasMaxLength(500);
        builder.Property(e => e.CreatedByUserId).HasMaxLength(450);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.PaidAtUtc });
        builder.HasIndex(e => new { e.OrganizationId, e.Status, e.ExpectedAtUtc });
        builder.HasIndex(e => new { e.OrganizationId, e.CampaignId, e.PaidAtUtc });
        builder.HasIndex(e => new { e.OrganizationId, e.DonationPlanId });
        builder.HasIndex(e => new { e.OrganizationId, e.ExternalPaymentId }).IsUnique().HasFilter("\"ExternalPaymentId\" IS NOT NULL");
    }
}
