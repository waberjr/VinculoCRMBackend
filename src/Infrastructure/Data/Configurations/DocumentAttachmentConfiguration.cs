using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DocumentAttachmentConfiguration : IEntityTypeConfiguration<DocumentAttachment>
{
    public void Configure(EntityTypeBuilder<DocumentAttachment> builder)
    {
        builder.Property(entity => entity.EntityType).HasMaxLength(80);
        builder.Property(entity => entity.Title).HasMaxLength(180);
        builder.Property(entity => entity.Url).HasMaxLength(1000);
        builder.Property(entity => entity.Description).HasMaxLength(1000);
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.EntityType, entity.EntityId });
    }
}
