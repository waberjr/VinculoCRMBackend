using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Token).HasMaxLength(120).IsRequired();
        builder.Property(e => e.InvitedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(e => e.AcceptedByUserId).HasMaxLength(450);
        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => new { e.OrganizationId, e.Email, e.AcceptedAtUtc });
        builder.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}
