using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageConfiguration : IEntityTypeConfiguration<LandingPage>
{
    public void Configure(EntityTypeBuilder<LandingPage> builder)
    {
        builder.Property(entity => entity.TargetType).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Subtitle).HasMaxLength(1000);
        builder.Property(entity => entity.HeroImageUrl).HasMaxLength(1000);
        builder.Property(entity => entity.CustomFieldsJson).HasMaxLength(4000);
        builder.Property(entity => entity.GoalAmount).HasPrecision(12, 2);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TargetType, entity.TargetId }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganizationId, entity.AppliedTemplateId });
    }
}
