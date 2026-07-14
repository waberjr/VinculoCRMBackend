using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageViewConfiguration : IEntityTypeConfiguration<LandingPageView>
{
    public void Configure(EntityTypeBuilder<LandingPageView> builder)
    {
        builder.Property(entity => entity.TargetType).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Source).HasMaxLength(120);
        builder.Property(entity => entity.UtmSource).HasMaxLength(120);
        builder.Property(entity => entity.UtmMedium).HasMaxLength(120);
        builder.Property(entity => entity.UtmCampaign).HasMaxLength(120);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TargetType, entity.TargetId, entity.ViewedAtUtc });
    }
}
