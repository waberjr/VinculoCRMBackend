using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class DonorPhoneConfiguration : IEntityTypeConfiguration<DonorPhone>
{
    public void Configure(EntityTypeBuilder<DonorPhone> builder)
    {
        builder.Property(e => e.Number).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(e => new { e.OrganizationId, e.DonorId });
        builder.HasIndex(e => new { e.OrganizationId, e.Number });
        builder.HasOne(e => e.Donor)
            .WithMany(e => e.Phones)
            .HasForeignKey(e => e.DonorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
