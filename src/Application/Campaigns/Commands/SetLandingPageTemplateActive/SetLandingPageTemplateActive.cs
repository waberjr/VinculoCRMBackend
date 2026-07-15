using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.SetLandingPageTemplateActive;

public sealed record SetLandingPageTemplateActiveCommand(Guid Id, bool IsActive) : IRequest;

public sealed class SetLandingPageTemplateActiveCommandHandler : IRequestHandler<SetLandingPageTemplateActiveCommand>
{
    private readonly IApplicationDbContext _context;

    public SetLandingPageTemplateActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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

        await _context.SaveChangesAsync(cancellationToken);
    }
}
