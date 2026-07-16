using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.RollbackLandingPageTemplate;

public sealed record RollbackLandingPageTemplateCommand(Guid Id, int Version) : IRequest;

public sealed class RollbackLandingPageTemplateCommandHandler : IRequestHandler<RollbackLandingPageTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public RollbackLandingPageTemplateCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RollbackLandingPageTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.LandingPageTemplates.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.Id.ToString());
        }

        var version = await _context.LandingPageTemplateVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.TemplateId == request.Id && entity.Version == request.Version, cancellationToken);
        if (version is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplateVersion), request.Version.ToString());
        }

        template.Update(
            version.Name,
            version.Title,
            version.Subtitle,
            version.HeroImageUrl,
            version.GoalAmount,
            version.CustomFieldsJson,
            version.IsActive,
            version.Category);

        _context.LandingPageTemplateVersions.Add(LandingPageTemplateSnapshots.FromTemplate(template, _timeProvider.GetUtcNow(), _user.Id));
        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            template.OrganizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "RolledBack",
            "Template restaurado",
            $"Restaurado a partir da versao {request.Version}. Nova versao: {template.Version}.",
            _timeProvider.GetUtcNow(),
            _user.Id));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
