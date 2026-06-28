using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;

public record CreateRelationshipTaskCommand : IRequest<Guid>
{
    public Guid DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? DonationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AssignedUserId { get; init; }
    public Guid TypeOptionId { get; init; }
    public Guid PriorityOptionId { get; init; }
    public Guid StatusOptionId { get; init; }
    public DateTimeOffset? DueAtUtc { get; init; }
}

public sealed class CreateRelationshipTaskCommandHandler : IRequestHandler<CreateRelationshipTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var task = new RelationshipTask
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            CampaignId = request.CampaignId,
            DonationId = request.DonationId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            AssignedUserId = request.AssignedUserId,
            CreatedByUserId = _user.Id,
            TypeOptionId = request.TypeOptionId,
            PriorityOptionId = request.PriorityOptionId,
            StatusOptionId = request.StatusOptionId,
            DueAtUtc = request.DueAtUtc,
        };

        _context.RelationshipTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
