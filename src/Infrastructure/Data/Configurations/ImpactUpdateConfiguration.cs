using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class ImpactUpdateConfiguration : IEntityTypeConfiguration<ImpactUpdate>
{
    public void Configure(EntityTypeBuilder<ImpactUpdate> builder)
    {
        builder.Property(entity => entity.Title).HasMaxLength(180);
        builder.Property(entity => entity.Content).HasMaxLength(4000);
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.ProjectId, entity.PublishedAtUtc });
        builder.HasOne(entity => entity.Project).WithMany().HasForeignKey(entity => entity.ProjectId).OnDelete(DeleteBehavior.Restrict);
    }
}
