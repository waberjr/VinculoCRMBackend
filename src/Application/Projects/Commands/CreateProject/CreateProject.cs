using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ImpactProjects.Commands.CreateProject;

public record CreateProjectCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? GoalAmount { get; init; }
    public string? ImpactMetric { get; init; }
    public string Status { get; init; } = "Draft";
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public IReadOnlyCollection<Guid> CampaignIds { get; init; } = [];
}

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CreateProjectCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var campaignIds = request.CampaignIds.Distinct().ToArray();

        if (campaignIds.Length > 0)
        {
            var existingCampaigns = await _context.Campaigns
                .AsNoTracking()
                .Select(campaign => campaign.Id)
                .ToListAsync(cancellationToken);

            if (campaignIds.Except(existingCampaigns).Any())
            {
                throw new Common.Exceptions.NotFoundException(nameof(Campaign), "Uma ou mais campanhas informadas nao foram encontradas.");
            }
        }

        var project = Project.Create(
            organizationId,
            request.Name,
            request.Description,
            request.GoalAmount,
            request.ImpactMetric,
            SystemOptionMapper.Parse<ProjectStatus>(request.Status),
            request.StartDateUtc,
            request.EndDateUtc);

        _context.Projects.Add(project);
        foreach (var campaignId in campaignIds)
        {
            _context.ProjectCampaigns.Add(new ProjectCampaign
            {
                OrganizationId = organizationId,
                ProjectId = project.Id,
                CampaignId = campaignId,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
