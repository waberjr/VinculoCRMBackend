namespace VinculoBackend.Domain.Entities;

public class ProjectCampaign : OrganizationEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
}
