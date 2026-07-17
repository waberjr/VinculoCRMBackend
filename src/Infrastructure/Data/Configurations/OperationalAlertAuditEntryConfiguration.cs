using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class OperationalAlertAuditEntryConfiguration : IEntityTypeConfiguration<OperationalAlertAuditEntry>
{
    public void Configure(EntityTypeBuilder<OperationalAlertAuditEntry> builder)
    {
        builder.Property(entity => entity.Action).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(600);
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.OperationalAlertId, entity.OccurredAtUtc });
    }
}
