using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageBlockRuleConfiguration : IEntityTypeConfiguration<LandingPageBlockRule>
{
    public void Configure(EntityTypeBuilder<LandingPageBlockRule> builder)
    {
        builder.Property(entity => entity.TargetType).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.FingerprintHash).HasMaxLength(128);
        builder.Property(entity => entity.Source).HasMaxLength(120);
        builder.Property(entity => entity.Reason).HasMaxLength(240);
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.ExpiresAtUtc);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TargetType, entity.TargetId, entity.IsActive });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TargetType, entity.TargetId, entity.FingerprintHash, entity.Source });
    }
}
