using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.CloneLandingPageTemplate;

public sealed record CloneLandingPageTemplateCommand(Guid Id, string? Name = null, string? Category = null) : IRequest<Guid>;

public sealed class CloneLandingPageTemplateCommandHandler : IRequestHandler<CloneLandingPageTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CloneLandingPageTemplateCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CloneLandingPageTemplateCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var source = await _context.LandingPageTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(template => template.Id == request.Id, cancellationToken);

        if (source is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.Id.ToString());
        }

        var clone = LandingPageTemplate.Create(
            organizationId,
            string.IsNullOrWhiteSpace(request.Name) ? $"{source.Name} - copia" : request.Name,
            source.Title,
            source.Subtitle,
            source.HeroImageUrl,
            source.GoalAmount,
            source.CustomFieldsJson,
            string.IsNullOrWhiteSpace(request.Category) ? source.Category : request.Category);

        _context.LandingPageTemplates.Add(clone);
        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            organizationId,
            nameof(LandingPageTemplate),
            clone.Id,
            "Cloned",
            "Template de landing clonado",
            $"{source.Name} -> {clone.Name}",
            _timeProvider.GetUtcNow(),
            _user.Id));

        await _context.SaveChangesAsync(cancellationToken);
        return clone.Id;
    }
}
