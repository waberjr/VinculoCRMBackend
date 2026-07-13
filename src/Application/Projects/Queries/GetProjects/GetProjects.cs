using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.ImpactProjects.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ImpactProjects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<PaginatedResult<ProjectListItemDto>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PaginatedResult<ProjectListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetProjectsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<ProjectListItemDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(project =>
                project.Name.ToLower().Contains(search) ||
                (project.Description != null && project.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<ProjectStatus>(request.Status);
            query = query.Where(project => project.Status == status);
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderBy(project => project.Status)
            .ThenByDescending(project => project.LastModified)
            .ThenBy(project => project.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(project => new
            {
                project.Id,
                project.Name,
                project.Description,
                project.Status,
                project.GoalAmount,
                project.ImpactMetric,
                project.StartDateUtc,
                project.EndDateUtc,
                UpdatedAtUtc = project.LastModified,
            })
            .ToListAsync(cancellationToken);

        var projectIds = projects.Select(project => project.Id).ToArray();
        var projectIdSet = projectIds.ToHashSet();
        var campaignsByProjectId = projectIds.Length == 0
            ? []
            : (await _context.ProjectCampaigns
                .AsNoTracking()
                .OrderBy(link => link.Campaign.Name)
                .Select(link => new
                {
                    link.ProjectId,
                    link.CampaignId,
                    CampaignName = link.Campaign.Name,
                })
                .ToListAsync(cancellationToken))
                .Where(link => projectIdSet.Contains(link.ProjectId))
                .ToList();

        var campaignLookup = campaignsByProjectId
            .GroupBy(link => link.ProjectId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<ProjectCampaignDto>)group
                    .Select(link => new ProjectCampaignDto
                    {
                        Id = link.CampaignId,
                        Name = link.CampaignName,
                    })
                    .ToList());

        var projectDonations = projectIds.Length == 0
            ? []
            : (await _context.DonationProjects
                .AsNoTracking()
                .Where(link =>
                    link.Donation.Status == DonationStatus.Confirmed)
                .Select(link => new
                {
                    link.ProjectId,
                    link.Donation.Amount,
                    link.Donation.DonorId,
                })
                .ToListAsync(cancellationToken))
                .Where(link => projectIdSet.Contains(link.ProjectId))
                .ToList();

        var metricsLookup = projectDonations
            .GroupBy(donation => donation.ProjectId)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    RaisedAmount = group.Sum(donation => donation.Amount),
                    DonorsCount = group.Select(donation => donation.DonorId).Distinct().Count(),
                });

        var items = projects
            .Select(project =>
            {
                campaignLookup.TryGetValue(project.Id, out var campaigns);
                metricsLookup.TryGetValue(project.Id, out var metrics);

                return new ProjectListItemDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Status = project.Status.ToString(),
                    GoalAmount = project.GoalAmount ?? 0,
                    RaisedAmount = metrics?.RaisedAmount ?? 0,
                    DonorsCount = metrics?.DonorsCount ?? 0,
                    ImpactMetric = project.ImpactMetric,
                    Campaigns = campaigns ?? [],
                    StartDateUtc = project.StartDateUtc,
                    EndDateUtc = project.EndDateUtc,
                    UpdatedAtUtc = project.UpdatedAtUtc,
                };
            })
            .ToList();

        return new PaginatedResult<ProjectListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = totalCount,
        };
    }
}
