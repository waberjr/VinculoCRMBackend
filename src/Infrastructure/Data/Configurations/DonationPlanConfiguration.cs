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
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.StatusOptionId });
        builder.HasOne(e => e.PreferredPaymentMethodOption).WithMany().HasForeignKey(e => e.PreferredPaymentMethodOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.StatusOption).WithMany().HasForeignKey(e => e.StatusOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
