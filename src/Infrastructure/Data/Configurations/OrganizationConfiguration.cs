using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LegalName).HasMaxLength(250);
        builder.Property(e => e.Document).HasMaxLength(32);
        builder.Property(e => e.DefaultMonthlyGoal).HasPrecision(12, 2);
        builder.Property(e => e.TimeZone).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
    }
}
