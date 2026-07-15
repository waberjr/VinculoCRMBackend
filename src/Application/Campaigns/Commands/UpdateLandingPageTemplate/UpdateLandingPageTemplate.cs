using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.UpdateLandingPageTemplate;

public sealed record UpdateLandingPageTemplateCommand(
    Guid Id,
    string Name,
    string? Category,
    string Title,
    string? Subtitle,
    string? HeroImageUrl,
    decimal? GoalAmount,
    bool IsActive,
    IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields) : IRequest;

public sealed class UpdateLandingPageTemplateCommandHandler : IRequestHandler<UpdateLandingPageTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateLandingPageTemplateCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateLandingPageTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.LandingPageTemplates
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.Id.ToString());
        }

        template.Update(
            request.Name,
            request.Title,
            request.Subtitle,
            request.HeroImageUrl,
            request.GoalAmount,
            LandingPageContent.SerializeFields(request.CustomFields),
            request.IsActive,
            request.Category);

        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            template.OrganizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "Updated",
            "Template de landing atualizado",
            template.Name,
            _timeProvider.GetUtcNow()));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
