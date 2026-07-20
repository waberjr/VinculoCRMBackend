namespace VinculoBackend.Application.OperationalAlerts.Models;

public sealed class OperationalAlertRuleDto
{
    public Guid Id { get; init; }
    public string Source { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public int WarningThreshold { get; init; }
    public int HighThreshold { get; init; }
    public int DueInHours { get; init; }
    public decimal? LowConversionThresholdPercent { get; init; }
    public bool IgnoreCancelledTasksForAutoResolution { get; init; }
    public string? AssignedUserId { get; init; }
}
