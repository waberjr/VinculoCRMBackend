using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.ImpactUpdates.Commands.CreateImpactUpdate;

public sealed record CreateImpactUpdateCommand(Guid ProjectId, string Title, string Content) : IRequest<Guid>;

public sealed class CreateImpactUpdateCommandHandler : IRequestHandler<CreateImpactUpdateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateImpactUpdateCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateImpactUpdateCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var projectExists = await _context.Projects.AsNoTracking().AnyAsync(project => project.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(Project), request.ProjectId.ToString());
        }

        var update = new ImpactUpdate
        {
            OrganizationId = organizationId,
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            PublishedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
        };

        _context.ImpactUpdates.Add(update);
        await _context.SaveChangesAsync(cancellationToken);

        return update.Id;
    }
}
