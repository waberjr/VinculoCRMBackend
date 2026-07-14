using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

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
    public FileUpload? HeroImage { get; init; }
}

public sealed class UpsertLandingPageConfigurationCommandHandler : IRequestHandler<UpsertLandingPageConfigurationCommand, LandingPageConfigurationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOrganizationContext _organizationContext;

    public UpsertLandingPageConfigurationCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _fileStorage = fileStorage;
        _organizationContext = organizationContext;
    }

    public async Task<LandingPageConfigurationDto> Handle(UpsertLandingPageConfigurationCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        await EnsureTargetExists(targetType, request.TargetId, cancellationToken);
        var heroImageUrl = await ResolveHeroImageUrl(organizationId, targetType, request, cancellationToken);

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
                heroImageUrl,
                request.GoalAmount,
                request.IsActive);
            _context.LandingPages.Add(page);
        }
        else
        {
            if (request.HeroImage is not null &&
                !string.IsNullOrWhiteSpace(page.HeroImageUrl) &&
                page.HeroImageUrl.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
            {
                await _fileStorage.DeleteAsync(page.HeroImageUrl, cancellationToken);
            }

            page.Update(request.Title, request.Subtitle, heroImageUrl, request.GoalAmount, request.IsActive);
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

    private async Task<string?> ResolveHeroImageUrl(
        Guid organizationId,
        string targetType,
        UpsertLandingPageConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        if (request.HeroImage is null)
        {
            return string.IsNullOrWhiteSpace(request.HeroImageUrl) ? null : request.HeroImageUrl.Trim();
        }

        ValidateHeroImage(request.HeroImage);
        var stored = await _fileStorage.StoreAsync(
            new StoreFileRequest(
                organizationId,
                "LandingPage",
                request.TargetId,
                request.HeroImage.FileName,
                request.HeroImage.ContentType,
                request.HeroImage.Content,
                request.HeroImage.Length),
            cancellationToken);

        return stored.InternalUri;
    }

    private static void ValidateHeroImage(FileUpload file)
    {
        var failures = new List<ValidationFailure>();
        if (file.Length <= 0)
        {
            failures.Add(new ValidationFailure(nameof(UpsertLandingPageConfigurationCommand.HeroImage), "Informe uma imagem valida."));
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            failures.Add(new ValidationFailure(nameof(UpsertLandingPageConfigurationCommand.HeroImage), "A imagem deve ter no maximo 5 MB."));
        }

        var allowedContentTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/svg+xml" };
        if (string.IsNullOrWhiteSpace(file.ContentType) || !allowedContentTypes.Contains(file.ContentType.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure(nameof(UpsertLandingPageConfigurationCommand.HeroImage), "Use PNG, JPG, WebP ou SVG."));
        }

        if (failures.Count > 0)
        {
            throw new Common.Exceptions.ValidationException(failures);
        }
    }
}
