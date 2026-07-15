using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.CreateLandingPageTemplate;

public sealed record CreateLandingPageTemplateCommand(
    string Name,
    string Title,
    string? Subtitle,
    string? HeroImageUrl,
    decimal? GoalAmount,
    IReadOnlyCollection<LandingPageCustomFieldDto> CustomFields) : IRequest<Guid>;

public sealed class CreateLandingPageTemplateCommandHandler : IRequestHandler<CreateLandingPageTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public CreateLandingPageTemplateCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateLandingPageTemplateCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var template = LandingPageTemplate.Create(
            organizationId,
            request.Name,
            request.Title,
            request.Subtitle,
            request.HeroImageUrl,
            request.GoalAmount,
            LandingPageContent.SerializeFields(request.CustomFields));

        _context.LandingPageTemplates.Add(template);
        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            organizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "Created",
            "Template de landing criado",
            template.Name,
            _timeProvider.GetUtcNow()));
        await _context.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
