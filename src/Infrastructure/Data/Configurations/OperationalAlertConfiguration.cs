using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class OperationalAlertConfiguration : IEntityTypeConfiguration<OperationalAlert>
{
    public void Configure(EntityTypeBuilder<OperationalAlert> builder)
    {
        builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(600);
        builder.Property(entity => entity.Source).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.RelatedEntityType).HasMaxLength(80);
        builder.Property(entity => entity.ActionUrl).HasMaxLength(360);
        builder.Property(entity => entity.AcknowledgedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.ResolvedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.ResolutionNote).HasMaxLength(600);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Status, entity.Severity, entity.OccurredAtUtc });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Source, entity.RelatedEntityType, entity.RelatedEntityId });
    }
}
