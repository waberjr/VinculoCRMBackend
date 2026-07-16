using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Campaigns.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.DeleteLandingPageTemplate;

public sealed record DeleteLandingPageTemplateCommand(Guid Id) : IRequest;

public sealed class DeleteLandingPageTemplateCommandHandler : IRequestHandler<DeleteLandingPageTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteLandingPageTemplateCommandHandler(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteLandingPageTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.LandingPageTemplates
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageTemplate), request.Id.ToString());
        }

        template.IsDeleted = true;
        template.Deleted = _timeProvider.GetUtcNow();
        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            template.OrganizationId,
            nameof(LandingPageTemplate),
            template.Id,
            "Deleted",
            "Template de landing excluido",
            template.Name,
            _timeProvider.GetUtcNow(),
            _user.Id));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
