using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class RelationshipTaskConfiguration : IEntityTypeConfiguration<RelationshipTask>
{
    public void Configure(EntityTypeBuilder<RelationshipTask> builder)
    {
        builder.ToTable("Tasks");
        builder.Property(e => e.Title).HasMaxLength(180).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.CompletionNote).HasMaxLength(1000);
        builder.Property(e => e.AssignedUserId).HasMaxLength(450);
        builder.Property(e => e.CreatedByUserId).HasMaxLength(450);
        builder.Property(e => e.OperationalAlertId);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.ContactOutcome).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(e => new { e.OrganizationId, e.AssignedUserId, e.Status, e.DueAtUtc });
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.Status });
        builder.HasIndex(e => new { e.OrganizationId, e.OperationalAlertId });
    }
}
