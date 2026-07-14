using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.RecordLandingPageView;

public sealed record RecordLandingPageViewCommand(
    string TargetType,
    Guid TargetId,
    string? Source,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign) : IRequest;

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

        _context.LandingPageViews.Add(new LandingPageView
        {
            OrganizationId = organizationId.Value,
            TargetType = targetType,
            TargetId = request.TargetId,
            Source = TrimToNull(request.Source),
            UtmSource = TrimToNull(request.UtmSource),
            UtmMedium = TrimToNull(request.UtmMedium),
            UtmCampaign = TrimToNull(request.UtmCampaign),
            ViewedAtUtc = _timeProvider.GetUtcNow(),
        });

        await _context.SaveChangesAsync(cancellationToken);
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
