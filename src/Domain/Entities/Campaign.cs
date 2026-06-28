namespace VinculoBackend.Domain.Entities;

public class Campaign : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TypeOptionId { get; set; }
    public ConfigurableOption TypeOption { get; set; } = null!;
    public Guid StatusOptionId { get; set; }
    public ConfigurableOption StatusOption { get; set; } = null!;
    public Guid? ChannelOptionId { get; set; }
    public ConfigurableOption? ChannelOption { get; set; }
    public decimal? GoalAmount { get; set; }
    public DateTimeOffset? StartDateUtc { get; set; }
    public DateTimeOffset? EndDateUtc { get; set; }
    public string? AssignedUserId { get; set; }
}
