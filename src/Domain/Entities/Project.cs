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
}
