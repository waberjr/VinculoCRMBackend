using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class OperationalProductivityGoalAuditEntryConfiguration : IEntityTypeConfiguration<OperationalProductivityGoalAuditEntry>
{
    public void Configure(EntityTypeBuilder<OperationalProductivityGoalAuditEntry> builder)
    {
        builder.Property(entity => entity.UserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ChangedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.UserId, entity.ChangedAtUtc });
    }
}
