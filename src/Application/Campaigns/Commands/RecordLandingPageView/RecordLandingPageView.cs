using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.RecordLandingPageView;

public sealed record RecordLandingPageViewCommand(
    string TargetType,
    Guid TargetId,
    string? Source,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? IpAddress,
    string? UserAgent) : IRequest;

public sealed class RecordLandingPageViewCommandHandler : IRequestHandler<RecordLandingPageViewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public RecordLandingPageViewCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RecordLandingPageViewCommand request, CancellationToken cancellationToken)
    {
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        var organizationId = await TargetOrganization(targetType, request.TargetId, cancellationToken);
        if (organizationId is null)
        {
            return;
        }

        var viewedAtUtc = _timeProvider.GetUtcNow();
        var windowStartedAtUtc = LandingPageViewDeduplication.Window(viewedAtUtc);
        var fingerprintHash = LandingPageViewDeduplication.Fingerprint(
            targetType,
            request.TargetId,
            TrimToNull(request.Source) ?? TrimToNull(request.UtmSource) ?? "landing",
            TrimToNull(request.IpAddress) ?? "unknown-ip",
            TrimToNull(request.UserAgent) ?? "unknown-agent");

        var alreadyRecorded = await _context.LandingPageViews
            .IgnoreQueryFilters()
            .AnyAsync(view =>
                !view.IsDeleted &&
                view.OrganizationId == organizationId.Value &&
                view.TargetType == targetType &&
                view.TargetId == request.TargetId &&
                view.FingerprintHash == fingerprintHash &&
                view.WindowStartedAtUtc == windowStartedAtUtc,
                cancellationToken);

        if (alreadyRecorded)
        {
            return;
        }

        _context.LandingPageViews.Add(new LandingPageView
        {
            OrganizationId = organizationId.Value,
            TargetType = targetType,
            TargetId = request.TargetId,
            FingerprintHash = fingerprintHash,
            Source = TrimToNull(request.Source),
            UtmSource = TrimToNull(request.UtmSource),
            UtmMedium = TrimToNull(request.UtmMedium),
            UtmCampaign = TrimToNull(request.UtmCampaign),
            WindowStartedAtUtc = windowStartedAtUtc,
            ViewedAtUtc = viewedAtUtc,
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Public landing metrics should be idempotent when the same visitor is recorded twice in the same window.
        }
    }

    private async Task<Guid?> TargetOrganization(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        if (targetType == "campaign")
        {
            return await _context.Campaigns
                .IgnoreQueryFilters()
                .Where(campaign => !campaign.IsDeleted && campaign.Id == targetId)
                .Select(campaign => (Guid?)campaign.OrganizationId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (targetType == "project")
        {
            return await _context.Projects
                .IgnoreQueryFilters()
                .Where(project => !project.IsDeleted && project.Id == targetId)
                .Select(project => (Guid?)project.OrganizationId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
