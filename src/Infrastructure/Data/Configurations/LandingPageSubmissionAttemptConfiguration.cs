using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageSubmissionAttemptConfiguration : IEntityTypeConfiguration<LandingPageSubmissionAttempt>
{
    public void Configure(EntityTypeBuilder<LandingPageSubmissionAttempt> builder)
    {
        builder.Property(entity => entity.TargetType).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.FingerprintHash).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Source).HasMaxLength(120);
        builder.Property(entity => entity.Reason).HasMaxLength(240);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TargetType, entity.TargetId, entity.FingerprintHash, entity.AttemptedAtUtc });
    }
}
