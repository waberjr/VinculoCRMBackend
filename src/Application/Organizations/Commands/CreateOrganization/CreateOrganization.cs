using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Organizations.Models;
using VinculoBackend.Application.Organizations.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(CreateOrganizationRequest Request, FileUpload? Logo = null) : IRequest<OrganizationResponse>;

public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationLogoService _logoService;
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public CreateOrganizationCommandHandler(
        IApplicationDbContext context,
        IOrganizationLogoService logoService,
        IOrganizationAdministrationService service,
        IUser user)
    {
        _context = context;
        _logoService = logoService;
        _service = service;
        _user = user;
    }

    public async Task<OrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(_user.Id ?? throw new UnauthorizedAccessException(), request.Request, cancellationToken);
        if (request.Logo is null)
        {
            return response;
        }

        await UpdateLogoAsync(response.Id, request.Logo, cancellationToken);
        return response with { LogoUrl = $"/api/Organizations/{response.Id}/Logo" };
    }

    private async Task UpdateLogoAsync(Guid organizationId, FileUpload logo, CancellationToken cancellationToken)
    {
        var logoUrl = await _logoService.StoreAsync(organizationId, logo, cancellationToken);
        var organization = await _context.Organizations.FirstOrDefaultAsync(item => item.Id == organizationId, cancellationToken)
            ?? throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(Organization), organizationId.ToString());

        organization.LogoUrl = logoUrl;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
