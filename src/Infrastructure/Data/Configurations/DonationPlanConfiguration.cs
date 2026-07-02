using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonationPlanConfiguration : IEntityTypeConfiguration<DonationPlan>
{
    public void Configure(EntityTypeBuilder<DonationPlan> builder)
    {
        builder.Property(e => e.ExpectedAmount).HasPrecision(12, 2);
        builder.Property(e => e.AssignedUserId).HasMaxLength(450);
        builder.Property(e => e.CancellationReason).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.PreferredPaymentMethod).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.Status });
    }
}
