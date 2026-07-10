using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ImpactProjects.Commands.UpdateProject;

public record UpdateProjectCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? GoalAmount { get; init; }
    public string? ImpactMetric { get; init; }
    public string Status { get; init; } = "Draft";
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public IReadOnlyCollection<Guid> CampaignIds { get; init; } = [];
}

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateProjectCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var project = await _context.Projects.FindAsync([request.Id], cancellationToken);
        if (project is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Project), request.Id.ToString());
        }

        project.Update(
            request.Name,
            request.Description,
            request.GoalAmount,
            request.ImpactMetric,
            SystemOptionMapper.Parse<ProjectStatus>(request.Status),
            request.StartDateUtc,
            request.EndDateUtc);

        await SyncCampaigns(project.Id, request.CampaignIds.Distinct().ToArray(), cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncCampaigns(Guid projectId, IReadOnlyCollection<Guid> campaignIds, CancellationToken cancellationToken)
    {
        if (campaignIds.Count > 0)
        {
            var existingCampaigns = await _context.Campaigns
                .AsNoTracking()
                .Where(campaign => campaignIds.Contains(campaign.Id))
                .Select(campaign => campaign.Id)
                .ToListAsync(cancellationToken);

            if (existingCampaigns.Count != campaignIds.Count)
            {
                throw new Common.Exceptions.NotFoundException(nameof(Campaign), "Uma ou mais campanhas informadas nao foram encontradas.");
            }
        }

        var currentLinks = await _context.ProjectCampaigns
            .Where(link => link.ProjectId == projectId)
            .ToListAsync(cancellationToken);
        var campaignIdsToKeep = campaignIds.ToHashSet();

        foreach (var link in currentLinks.Where(link => !campaignIdsToKeep.Contains(link.CampaignId)))
        {
            link.IsDeleted = true;
        }

        var currentCampaignIds = currentLinks
            .Where(link => !link.IsDeleted)
            .Select(link => link.CampaignId)
            .ToHashSet();

        var organizationId = RequiredOrganization.From(_organizationContext);
        foreach (var campaignId in campaignIds.Where(campaignId => !currentCampaignIds.Contains(campaignId)))
        {
            _context.ProjectCampaigns.Add(new ProjectCampaign
            {
                OrganizationId = organizationId,
                ProjectId = projectId,
                CampaignId = campaignId,
            });
        }
    }
}
