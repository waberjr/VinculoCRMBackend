using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class OperationalAlertRuleConfiguration : IEntityTypeConfiguration<OperationalAlertRule>
{
    public void Configure(EntityTypeBuilder<OperationalAlertRule> builder)
    {
        builder.Property(entity => entity.Source).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.AssignedUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Source }).IsUnique();
    }
}
