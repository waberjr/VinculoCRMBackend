using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Organizations.Models;
using VinculoBackend.Application.Organizations.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(Guid Id, UpdateOrganizationRequest Request, FileUpload? Logo = null) : IRequest;

public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationLogoService _logoService;
    private readonly IOrganizationAdministrationService _service;
    private readonly IUser _user;

    public UpdateOrganizationCommandHandler(
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

    public async Task Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        await _service.UpdateAsync(_user.Id ?? throw new UnauthorizedAccessException(), request.Id, request.Request, cancellationToken);
        if (request.Logo is null)
        {
            return;
        }

        var logoUrl = await _logoService.StoreAsync(request.Id, request.Logo, cancellationToken);
        var organization = await _context.Organizations.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(Organization), request.Id.ToString());

        organization.LogoUrl = logoUrl;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
