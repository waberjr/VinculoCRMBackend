using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class ConfigurableOptionConfiguration : IEntityTypeConfiguration<ConfigurableOption>
{
    public void Configure(EntityTypeBuilder<ConfigurableOption> builder)
    {
        builder.Property(e => e.Category).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Color).HasMaxLength(32);
        builder.HasIndex(e => new { e.OrganizationId, e.Category, e.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(e => new { e.OrganizationId, e.Category, e.IsActive });
    }
}
