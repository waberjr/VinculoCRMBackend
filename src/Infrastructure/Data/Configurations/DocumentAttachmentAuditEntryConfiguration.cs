using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DocumentAttachmentAuditEntryConfiguration : IEntityTypeConfiguration<DocumentAttachmentAuditEntry>
{
    public void Configure(EntityTypeBuilder<DocumentAttachmentAuditEntry> builder)
    {
        builder.Property(entity => entity.EntityType).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Action).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.DocumentAttachmentId, entity.OccurredAtUtc });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.EntityType, entity.EntityId });
    }
}
