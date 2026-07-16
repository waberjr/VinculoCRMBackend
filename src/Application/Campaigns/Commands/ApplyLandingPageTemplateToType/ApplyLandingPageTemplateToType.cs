using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.ApplyLandingPageTemplateToType;

public sealed record ApplyLandingPageTemplateToTypeCommand(Guid TemplateId, string TargetType) : IRequest<ApplyLandingPageTemplateResultDto>;

public sealed class ApplyLandingPageTemplateToTypeCommandHandler : IRequestHandler<ApplyLandingPageTemplateToTypeCommand, ApplyLandingPageTemplateResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public ApplyLandingPageTemplateToTypeCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<ApplyLandingPageTemplateResultDto> Handle(ApplyLandingPageTemplateToTypeCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var targetType = NormalizeTargetType(request.TargetType);
        var template = await _context.LandingPageTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.TemplateId, cancellationToken);

        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.TemplateId.ToString());
        }

        var pages = await _context.LandingPages
            .Where(page => page.TargetType == targetType)
            .ToArrayAsync(cancellationToken);

        foreach (var page in pages)
        {
            page.Update(
                template.Title,
                template.Subtitle,
                template.HeroImageUrl,
                template.GoalAmount,
                page.IsActive,
                page.IsPublished,
                template.CustomFieldsJson,
                page.PublishedAtUtc,
                template.Id);
        }

        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            organizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "BulkApplied",
            "Template aplicado em massa",
            $"{template.Name} aplicado em {pages.Length} landing(s) do tipo {targetType}.",
            _timeProvider.GetUtcNow(),
            _user.Id));

        await _context.SaveChangesAsync(cancellationToken);
        return new ApplyLandingPageTemplateResultDto { UpdatedCount = pages.Length };
    }

    private static string NormalizeTargetType(string targetType)
    {
        return targetType.Trim().ToLowerInvariant() switch
        {
            "campaign" => "campaign",
            "project" => "project",
            _ => throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException(
                [new FluentValidation.Results.ValidationFailure(nameof(ApplyLandingPageTemplateToTypeCommand.TargetType), "Tipo de landing invalido.")]),
        };
    }
}
