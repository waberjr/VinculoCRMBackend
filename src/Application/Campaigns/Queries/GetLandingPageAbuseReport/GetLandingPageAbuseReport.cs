using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageAbuseReport;

public sealed record GetLandingPageAbuseReportQuery(
    string? TargetType = null,
    Guid? TargetId = null,
    string? Source = null,
    bool? Blocked = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null,
    int Limit = 100) : IRequest<LandingPageAbuseReportDto>;

public sealed class GetLandingPageAbuseReportQueryHandler : IRequestHandler<GetLandingPageAbuseReportQuery, LandingPageAbuseReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetLandingPageAbuseReportQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<LandingPageAbuseReportDto> Handle(GetLandingPageAbuseReportQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);
        var now = _timeProvider.GetUtcNow();

        var query = _context.LandingPageSubmissionAttempts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.TargetType))
        {
            var targetType = request.TargetType.Trim().ToLowerInvariant();
            query = query.Where(attempt => attempt.TargetType == targetType);
        }

        if (request.TargetId is not null)
        {
            query = query.Where(attempt => attempt.TargetId == request.TargetId);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim();
            query = query.Where(attempt => attempt.Source == source);
        }

        if (request.Blocked is not null)
        {
            query = query.Where(attempt => attempt.Blocked == request.Blocked);
        }

        if (request.StartDateUtc is not null)
        {
            query = query.Where(attempt => attempt.AttemptedAtUtc >= request.StartDateUtc);
        }

        if (request.EndDateUtc is not null)
        {
            query = query.Where(attempt => attempt.AttemptedAtUtc <= request.EndDateUtc);
        }

        var attemptsCount = await query.CountAsync(cancellationToken);
        var blockedCount = await query.CountAsync(attempt => attempt.Blocked, cancellationToken);
        var limit = request.Limit <= 0 ? 100 : Math.Min(request.Limit, 300);
        var attempts = await query
            .OrderByDescending(attempt => attempt.AttemptedAtUtc)
            .Take(limit)
            .Select(attempt => new
            {
                attempt.Id,
                attempt.TargetType,
                attempt.TargetId,
                attempt.FingerprintHash,
                attempt.Source,
                attempt.Blocked,
                attempt.Reason,
                attempt.AttemptedAtUtc,
            })
            .ToArrayAsync(cancellationToken);

        var campaignIds = attempts
            .Where(attempt => attempt.TargetType == "campaign")
            .Select(attempt => attempt.TargetId)
            .Distinct()
            .ToArray();
        var projectIds = attempts
            .Where(attempt => attempt.TargetType == "project")
            .Select(attempt => attempt.TargetId)
            .Distinct()
            .ToArray();

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

        var activeRules = await _context.LandingPageBlockRules
            .AsNoTracking()
            .Where(rule => rule.IsActive && (rule.ExpiresAtUtc == null || rule.ExpiresAtUtc > now))
            .Select(rule => new
            {
                rule.Id,
                rule.TargetType,
                rule.TargetId,
                rule.FingerprintHash,
                rule.Source,
            })
            .ToArrayAsync(cancellationToken);

        return new LandingPageAbuseReportDto
        {
            AttemptsCount = attemptsCount,
            BlockedCount = blockedCount,
            Items = attempts.Select(attempt => new LandingPageAbuseReportItemDto
            {
                Id = attempt.Id,
                TargetType = attempt.TargetType,
                TargetId = attempt.TargetId,
                TargetName = TargetName(attempt.TargetType, attempt.TargetId, campaignNames, projectNames),
                Source = attempt.Source,
                FingerprintHash = attempt.FingerprintHash,
                ActiveBlockRuleId = activeRules
                    .FirstOrDefault(rule =>
                        rule.TargetType == attempt.TargetType &&
                        rule.TargetId == attempt.TargetId &&
                        ((rule.FingerprintHash != null && rule.FingerprintHash == attempt.FingerprintHash) ||
                         (rule.Source != null && rule.Source == attempt.Source)))?.Id,
                Blocked = attempt.Blocked,
                Reason = attempt.Reason,
                AttemptedAtUtc = attempt.AttemptedAtUtc,
            }).ToArray(),
        };
    }

    private static string TargetName(
        string targetType,
        Guid targetId,
        IReadOnlyDictionary<Guid, string> campaignNames,
        IReadOnlyDictionary<Guid, string> projectNames)
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
