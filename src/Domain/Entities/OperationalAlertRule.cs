using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class OperationalAlertRule : OrganizationEntity
{
    public string Source { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public int WarningThreshold { get; private set; } = 1;
    public int HighThreshold { get; private set; } = 10;
    public int DueInHours { get; private set; } = 24;
    public string? AssignedUserId { get; private set; }

    public static OperationalAlertRule Create(
        Guid organizationId,
        string source,
        bool isEnabled,
        int warningThreshold,
        int highThreshold,
        int dueInHours,
        string? assignedUserId)
    {
        var rule = new OperationalAlertRule
        {
            OrganizationId = organizationId,
            Source = Required(source, "Informe a origem da regra."),
        };
        rule.Update(isEnabled, warningThreshold, highThreshold, dueInHours, assignedUserId);
        return rule;
    }

    public void Update(bool isEnabled, int warningThreshold, int highThreshold, int dueInHours, string? assignedUserId)
    {
        if (warningThreshold < 1)
        {
            throw new DomainValidationException("O limite de alerta deve ser maior que zero.");
        }

        if (highThreshold < warningThreshold)
        {
            throw new DomainValidationException("O limite de alerta alto deve ser maior ou igual ao limite inicial.");
        }

        if (dueInHours is < 1 or > 720)
        {
            throw new DomainValidationException("O prazo deve estar entre 1 e 720 horas.");
        }

        IsEnabled = isEnabled;
        WarningThreshold = warningThreshold;
        HighThreshold = highThreshold;
        DueInHours = dueInHours;
        AssignedUserId = TrimToNull(assignedUserId);
    }

    public OperationalAlertSeverity SeverityFor(int count) =>
        count >= HighThreshold ? OperationalAlertSeverity.High : OperationalAlertSeverity.Warning;

    public bool ShouldAlert(int count) => IsEnabled && count >= WarningThreshold;

    private static string Required(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(message);
        }

        return value.Trim();
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
