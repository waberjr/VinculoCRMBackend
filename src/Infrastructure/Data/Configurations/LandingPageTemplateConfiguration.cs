using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageTemplateConfiguration : IEntityTypeConfiguration<LandingPageTemplate>
{
    public void Configure(EntityTypeBuilder<LandingPageTemplate> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Subtitle).HasMaxLength(600);
        builder.Property(entity => entity.HeroImageUrl).HasMaxLength(1000);
        builder.Property(entity => entity.GoalAmount).HasPrecision(12, 2);
        builder.Property(entity => entity.CustomFieldsJson).HasColumnType("json");
        builder.HasIndex(entity => new { entity.OrganizationId, entity.IsActive, entity.Name });
    }
}
