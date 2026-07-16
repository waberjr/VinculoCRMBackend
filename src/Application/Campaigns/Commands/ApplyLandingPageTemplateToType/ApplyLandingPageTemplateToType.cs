using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.ApplyLandingPageTemplateToType;

public sealed record ApplyLandingPageTemplateToTypeCommand(
    Guid TemplateId,
    string TargetType,
    bool OnlyActive = false,
    bool OnlyPublished = false) : IRequest<ApplyLandingPageTemplateResultDto>;

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

        var pagesQuery = _context.LandingPages.Where(page => page.TargetType == targetType);
        if (request.OnlyActive)
        {
            pagesQuery = pagesQuery.Where(page => page.IsActive);
        }

        if (request.OnlyPublished)
        {
            pagesQuery = pagesQuery.Where(page => page.IsPublished);
        }

        var pages = await pagesQuery.ToArrayAsync(cancellationToken);

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
                template.Id,
                page.SubmissionLimitWindowMinutes,
                page.SubmissionLimitMaxAttempts);
        }

        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            organizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "BulkApplied",
            "Template aplicado em massa",
            Description(template.Name, pages.Length, targetType, request.OnlyActive, request.OnlyPublished),
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

    private static string Description(string templateName, int updatedCount, string targetType, bool onlyActive, bool onlyPublished)
    {
        var filters = new List<string>();
        if (onlyActive)
        {
            filters.Add("ativas");
        }

        if (onlyPublished)
        {
            filters.Add("publicadas");
        }

        var filterText = filters.Count == 0 ? "sem filtros" : $"filtrando {string.Join(" e ", filters)}";
        return $"{templateName} aplicado em {updatedCount} landing(s) do tipo {targetType}, {filterText}.";
    }
}
