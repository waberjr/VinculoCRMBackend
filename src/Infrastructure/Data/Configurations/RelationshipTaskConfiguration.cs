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
        builder.HasIndex(e => new { e.OrganizationId, e.AssignedUserId, e.StatusOptionId, e.DueAtUtc });
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId, e.StatusOptionId });
    }
}
