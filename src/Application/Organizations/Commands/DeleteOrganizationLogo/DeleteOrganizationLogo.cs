using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Security;
using VinculoBackend.Domain.Constants;

namespace VinculoBackend.Application.Organizations.Commands.DeleteOrganizationLogo;

[Authorize(Roles = Roles.SystemAdministrator)]
public sealed record DeleteOrganizationLogoCommand(Guid OrganizationId) : IRequest;

public sealed class DeleteOrganizationLogoCommandHandler : IRequestHandler<DeleteOrganizationLogoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public DeleteOrganizationLogoCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(DeleteOrganizationLogoCommand request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(entity => entity.Id == request.OrganizationId, cancellationToken);

        if (organization is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(organization), request.OrganizationId.ToString());
        }

        if (!string.IsNullOrWhiteSpace(organization.LogoUrl))
        {
            if (organization.LogoUrl.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
            {
                await _fileStorageService.DeleteAsync(organization.LogoUrl, cancellationToken);
            }

            organization.LogoUrl = null;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
