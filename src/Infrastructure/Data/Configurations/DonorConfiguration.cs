using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorConfiguration : IEntityTypeConfiguration<Donor>
{
    public void Configure(EntityTypeBuilder<Donor> builder)
    {
        builder.Property(e => e.FullName).HasMaxLength(250).IsRequired();
        builder.Property(e => e.Document).HasMaxLength(32);
        builder.Property(e => e.Email).HasMaxLength(254);
        builder.Property(e => e.Phone).HasMaxLength(32);
        builder.Property(e => e.WhatsApp).HasMaxLength(32);
        builder.Property(e => e.City).HasMaxLength(120);
        builder.Property(e => e.State).HasMaxLength(40);
        builder.Property(e => e.AddressLine1).HasMaxLength(250);
        builder.Property(e => e.AddressLine2).HasMaxLength(250);
        builder.Property(e => e.PostalCode).HasMaxLength(20);
        builder.Property(e => e.AssignedUserId).HasMaxLength(450);
        builder.Property(e => e.DoNotContactReason).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.HasIndex(e => new { e.OrganizationId, e.FullName });
        builder.HasIndex(e => new { e.OrganizationId, e.Document }).HasFilter("\"Document\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.Email }).HasFilter("\"Email\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.Phone }).HasFilter("\"Phone\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.StatusOptionId });
        builder.HasIndex(e => new { e.OrganizationId, e.AssignedUserId });
    }
}
