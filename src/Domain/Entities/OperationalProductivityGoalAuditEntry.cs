namespace VinculoBackend.Domain.Entities;

public class OperationalProductivityGoalAuditEntry : OrganizationEntity
{
    public string UserId { get; set; } = string.Empty;
    public int? PreviousTaskGoalMonthly { get; set; }
    public int? NewTaskGoalMonthly { get; set; }
    public int? PreviousSlaHours { get; set; }
    public int? NewSlaHours { get; set; }
    public string? ChangedByUserId { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}
