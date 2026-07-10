using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class Project : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? GoalAmount { get; private set; }
    public string? ImpactMetric { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTimeOffset? StartDateUtc { get; private set; }
    public DateTimeOffset? EndDateUtc { get; private set; }

    public static Project Create(
        Guid organizationId,
        string name,
        string? description,
        decimal? goalAmount,
        string? impactMetric,
        ProjectStatus status,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc)
    {
        var project = new Project { OrganizationId = organizationId };
        project.Update(name, description, goalAmount, impactMetric, status, startDateUtc, endDateUtc);

        return project;
    }

    public void Update(
        string name,
        string? description,
        decimal? goalAmount,
        string? impactMetric,
        ProjectStatus status,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc)
    {
        SetName(name);
        Description = TrimToNull(description);
        ImpactMetric = TrimToNull(impactMetric);
        Status = status;
        SetGoalAmount(goalAmount);
        SetPeriod(startDateUtc, endDateUtc);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Informe o nome do projeto.");
        }

        Name = name.Trim();
    }

    public void SetGoalAmount(decimal? goalAmount)
    {
        if (goalAmount is not null && goalAmount <= 0)
        {
            throw new DomainValidationException("A meta do projeto deve ser maior que zero.");
        }

        GoalAmount = goalAmount;
    }

    public void SetPeriod(DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
    {
        if (startDateUtc is not null && endDateUtc is not null && startDateUtc >= endDateUtc)
        {
            throw new DomainValidationException("A data de termino do projeto deve ser maior que a data de inicio.");
        }

        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
