using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class Campaign : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; }
    public CampaignChannel? Channel { get; set; }
    public decimal? GoalAmount { get; set; }
    public DateTimeOffset? StartDateUtc { get; private set; }
    public DateTimeOffset? EndDateUtc { get; private set; }
    public string? AssignedUserId { get; set; }

    public static Campaign Create(
        Guid organizationId,
        string name,
        string? description,
        CampaignType type,
        CampaignChannel? channel,
        decimal? goalAmount,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc,
        string? assignedUserId)
    {
        var campaign = new Campaign
        {
            OrganizationId = organizationId,
            Status = CampaignStatus.Draft,
            AssignedUserId = assignedUserId,
        };
        campaign.Update(name, description, type, channel, goalAmount, startDateUtc, endDateUtc);

        return campaign;
    }

    public void Update(
        string name,
        string? description,
        CampaignType type,
        CampaignChannel? channel,
        decimal? goalAmount,
        DateTimeOffset? startDateUtc,
        DateTimeOffset? endDateUtc)
    {
        SetName(name);
        Description = TrimToNull(description);
        Type = type;
        Channel = channel;
        SetGoalAmount(goalAmount);
        SetPeriod(startDateUtc, endDateUtc);
    }

    public void Activate()
    {
        if (Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Campanhas encerradas ou canceladas nao podem ser ativadas.");
        }

        Status = CampaignStatus.Active;
    }

    public void Complete()
    {
        if (Status == CampaignStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Campanhas canceladas nao podem ser encerradas.");
        }

        Status = CampaignStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == CampaignStatus.Completed)
        {
            throw new InvalidOperationDomainException("Campanhas encerradas nao podem ser canceladas.");
        }

        Status = CampaignStatus.Cancelled;
    }

    public void SetGoalAmount(decimal? goalAmount)
    {
        if (goalAmount is null or <= 0)
        {
            throw new DomainValidationException("A meta da campanha deve ser maior que zero.");
        }

        GoalAmount = goalAmount;
    }

    public void SetPeriod(DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
    {
        if (startDateUtc is not null && endDateUtc is not null && startDateUtc >= endDateUtc)
        {
            throw new DomainValidationException("A data de termino da campanha deve ser maior que a data de inicio.");
        }

        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Informe o nome da campanha.");
        }

        Name = name.Trim();
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
