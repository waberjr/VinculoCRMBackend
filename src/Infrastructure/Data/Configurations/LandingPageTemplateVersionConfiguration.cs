using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public sealed class LandingPageTemplateVersionConfiguration : IEntityTypeConfiguration<LandingPageTemplateVersion>
{
    public void Configure(EntityTypeBuilder<LandingPageTemplateVersion> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Category).HasMaxLength(80);
        builder.Property(entity => entity.Title).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Subtitle).HasMaxLength(600);
        builder.Property(entity => entity.HeroImageUrl).HasMaxLength(1000);
        builder.Property(entity => entity.GoalAmount).HasPrecision(12, 2);
        builder.Property(entity => entity.CustomFieldsJson).HasColumnType("json");
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.TemplateId, entity.Version }).IsUnique();
    }
}
