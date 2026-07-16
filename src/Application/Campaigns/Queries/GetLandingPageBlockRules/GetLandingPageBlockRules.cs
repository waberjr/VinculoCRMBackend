using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageBlockRules;

public sealed record GetLandingPageBlockRulesQuery(
    string? TargetType = null,
    Guid? TargetId = null,
    bool IncludeInactive = false,
    bool IncludeExpired = true,
    string? Source = null,
    string? FingerprintHash = null,
    bool? Active = null,
    bool? Expired = null) : IRequest<IReadOnlyCollection<LandingPageBlockRuleDto>>;

public sealed class GetLandingPageBlockRulesQueryHandler : IRequestHandler<GetLandingPageBlockRulesQuery, IReadOnlyCollection<LandingPageBlockRuleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetLandingPageBlockRulesQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<LandingPageBlockRuleDto>> Handle(GetLandingPageBlockRulesQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var now = _timeProvider.GetUtcNow();
        var query = _context.LandingPageBlockRules.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.TargetType))
        {
            var targetType = request.TargetType.Trim().ToLowerInvariant();
            query = query.Where(rule => rule.TargetType == targetType);
        }

        if (request.TargetId is not null)
        {
            query = query.Where(rule => rule.TargetId == request.TargetId);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim();
            query = query.Where(rule => rule.Source != null && rule.Source.Contains(source));
        }

        if (!string.IsNullOrWhiteSpace(request.FingerprintHash))
        {
            var fingerprintHash = request.FingerprintHash.Trim();
            query = query.Where(rule => rule.FingerprintHash != null && rule.FingerprintHash.Contains(fingerprintHash));
        }

        if (request.Active is not null)
        {
            query = query.Where(rule => rule.IsActive == request.Active);
        }

        if (request.Expired is not null)
        {
            query = request.Expired.Value
                ? query.Where(rule => rule.ExpiresAtUtc != null && rule.ExpiresAtUtc <= now)
                : query.Where(rule => rule.ExpiresAtUtc == null || rule.ExpiresAtUtc > now);
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(rule => rule.IsActive);
        }

        if (!request.IncludeExpired)
        {
            query = query.Where(rule => rule.ExpiresAtUtc == null || rule.ExpiresAtUtc > now);
        }

        var rules = await query
            .OrderByDescending(rule => rule.CreatedAtUtc)
            .Take(200)
            .Select(rule => new
            {
                rule.Id,
                rule.TargetType,
                rule.TargetId,
                rule.FingerprintHash,
                rule.Source,
                rule.Reason,
                rule.IsActive,
                rule.ExpiresAtUtc,
                rule.CreatedAtUtc,
            })
            .ToArrayAsync(cancellationToken);

        var campaignIds = rules.Where(rule => rule.TargetType == "campaign").Select(rule => rule.TargetId).Distinct().ToArray();
        var projectIds = rules.Where(rule => rule.TargetType == "project").Select(rule => rule.TargetId).Distinct().ToArray();
        var campaignNames = await _context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaignIds.Contains(campaign.Id))
            .Select(campaign => new { campaign.Id, campaign.Name })
            .ToDictionaryAsync(campaign => campaign.Id, campaign => campaign.Name, cancellationToken);
        var projectNames = await _context.Projects
            .AsNoTracking()
            .Where(project => projectIds.Contains(project.Id))
            .Select(project => new { project.Id, project.Name })
            .ToDictionaryAsync(project => project.Id, project => project.Name, cancellationToken);

        return rules.Select(rule => new LandingPageBlockRuleDto
        {
            Id = rule.Id,
            TargetType = rule.TargetType,
            TargetId = rule.TargetId,
            TargetName = TargetName(rule.TargetType, rule.TargetId, campaignNames, projectNames),
            FingerprintHash = rule.FingerprintHash,
            Source = rule.Source,
            Reason = rule.Reason,
            IsActive = rule.IsActive,
            IsExpired = rule.ExpiresAtUtc is not null && rule.ExpiresAtUtc <= now,
            ExpiresAtUtc = rule.ExpiresAtUtc,
            CreatedAtUtc = rule.CreatedAtUtc,
        }).ToArray();
    }

    private static string TargetName(string targetType, Guid targetId, IReadOnlyDictionary<Guid, string> campaignNames, IReadOnlyDictionary<Guid, string> projectNames)
    {
        if (targetType == "campaign" && campaignNames.TryGetValue(targetId, out var campaignName))
        {
            return campaignName;
        }

        if (targetType == "project" && projectNames.TryGetValue(targetId, out var projectName))
        {
            return projectName;
        }

        return $"{targetType}/{targetId}";
    }
}
