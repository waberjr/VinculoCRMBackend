using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.UpsertLandingPageConfiguration;

public sealed record UpsertLandingPageConfigurationCommand : IRequest<LandingPageConfigurationDto>
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public decimal? GoalAmount { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpsertLandingPageConfigurationCommandHandler : IRequestHandler<UpsertLandingPageConfigurationCommand, LandingPageConfigurationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpsertLandingPageConfigurationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPageConfigurationDto> Handle(UpsertLandingPageConfigurationCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        await EnsureTargetExists(targetType, request.TargetId, cancellationToken);

        var page = await _context.LandingPages
            .FirstOrDefaultAsync(entity => entity.TargetType == targetType && entity.TargetId == request.TargetId, cancellationToken);
        if (page is null)
        {
            page = LandingPage.Create(
                organizationId,
                targetType,
                request.TargetId,
                request.Title,
                request.Subtitle,
                request.HeroImageUrl,
                request.GoalAmount,
                request.IsActive);
            _context.LandingPages.Add(page);
        }
        else
        {
            page.Update(request.Title, request.Subtitle, request.HeroImageUrl, request.GoalAmount, request.IsActive);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new LandingPageConfigurationDto
        {
            TargetType = page.TargetType,
            TargetId = page.TargetId,
            Title = page.Title,
            Subtitle = page.Subtitle,
            HeroImageUrl = page.HeroImageUrl,
            GoalAmount = page.GoalAmount,
            IsActive = page.IsActive,
        };
    }

    private async Task EnsureTargetExists(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var exists = targetType switch
        {
            "campaign" => await _context.Campaigns.AsNoTracking().AnyAsync(campaign => campaign.Id == targetId, cancellationToken),
            "project" => await _context.Projects.AsNoTracking().AnyAsync(project => project.Id == targetId, cancellationToken),
            _ => false,
        };

        if (!exists)
        {
            throw new Common.Exceptions.NotFoundException(targetType, targetId.ToString());
        }
    }
}
