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
    public DateTimeOffset? StartDateUtc { get; private set; }
    public DateTimeOffset? EndDateUtc { get; private set; }
    public string? AssignedUserId { get; set; }

    public void SetPeriod(DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
    {
        if (startDateUtc is not null && endDateUtc is not null && startDateUtc >= endDateUtc)
        {
            throw new ArgumentException("Campaign end date must be greater than start date.", nameof(endDateUtc));
        }

        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
    }
}
