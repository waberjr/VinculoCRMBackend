using VinculoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VinculoBackend.Infrastructure.Data.Configurations;

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Channel).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Subject).HasMaxLength(180);
        builder.Property(entity => entity.Body).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.Variables).HasMaxLength(1000);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.Channel, entity.IsActive });
    }
}
