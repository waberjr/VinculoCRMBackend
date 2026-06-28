using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
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
    public string Type { get; init; } = "Call";
    public string Priority { get; init; } = "Medium";
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

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

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
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskType", request.Type, cancellationToken),
            PriorityOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskPriority", request.Priority, cancellationToken),
            StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "TaskStatus", "Open", cancellationToken),
            DueAtUtc = request.DueAtUtc,
        };

        _context.RelationshipTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
