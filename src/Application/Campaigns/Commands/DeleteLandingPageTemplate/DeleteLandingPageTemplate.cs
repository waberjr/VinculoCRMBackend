using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.DeleteLandingPageTemplate;

public sealed record DeleteLandingPageTemplateCommand(Guid Id) : IRequest;

public sealed class DeleteLandingPageTemplateCommandHandler : IRequestHandler<DeleteLandingPageTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteLandingPageTemplateCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
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
        await _context.SaveChangesAsync(cancellationToken);
    }
}
