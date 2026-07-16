using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.SetLandingPageTemplateActive;

public sealed record SetLandingPageTemplateActiveCommand(Guid Id, bool IsActive) : IRequest;

public sealed class SetLandingPageTemplateActiveCommandHandler : IRequestHandler<SetLandingPageTemplateActiveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public SetLandingPageTemplateActiveCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(SetLandingPageTemplateActiveCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.LandingPageTemplates
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.Id.ToString());
        }

        if (request.IsActive)
        {
            template.Activate();
        }
        else
        {
            template.Deactivate();
        }

        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            template.OrganizationId,
            nameof(LandingPageTemplate),
            template.Id,
            request.IsActive ? "Activated" : "Deactivated",
            request.IsActive ? "Template de landing ativado" : "Template de landing desativado",
            template.Name,
            _timeProvider.GetUtcNow(),
            _user.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
