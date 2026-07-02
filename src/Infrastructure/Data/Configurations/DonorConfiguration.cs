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
        builder.Property(e => e.PersonType).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.FullName });
        builder.HasIndex(e => new { e.OrganizationId, e.Document })
            .IsUnique()
            .HasFilter("\"Document\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.Email }).HasFilter("\"Email\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.Phone }).HasFilter("\"Phone\" IS NOT NULL");
        builder.HasIndex(e => new { e.OrganizationId, e.Status });
        builder.HasIndex(e => new { e.OrganizationId, e.AssignedUserId });
        builder.HasOne(e => e.SourceOption).WithMany().HasForeignKey(e => e.SourceOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.RelationshipProfileOption).WithMany().HasForeignKey(e => e.RelationshipProfileOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.PreferredContactChannelOption).WithMany().HasForeignKey(e => e.PreferredContactChannelOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
