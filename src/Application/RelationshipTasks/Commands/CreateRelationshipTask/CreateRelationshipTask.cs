using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

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
    public bool ConfirmBlockedContact { get; init; }
    public string? BlockedContactJustification { get; init; }
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

        var donor = await _context.Donors
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.DonorId, cancellationToken);
        if (donor is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        if (donor.DoNotContact || !donor.AllowsCommunication)
        {
            if (!request.ConfirmBlockedContact || string.IsNullOrWhiteSpace(request.BlockedContactJustification))
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(CreateRelationshipTaskCommand.DonorId), "Doador bloqueado para contato exige confirmacao explicita e justificativa."),
                ]);
            }
        }

        if (request.CampaignId is not null &&
            !await _context.Campaigns.AsNoTracking().AnyAsync(campaign => campaign.Id == request.CampaignId, cancellationToken))
        {
            throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.CampaignId.Value.ToString());
        }

        if (request.DonationId is not null)
        {
            var donationBelongsToDonor = await _context.Donations
                .AsNoTracking()
                .AnyAsync(donation => donation.Id == request.DonationId && donation.DonorId == request.DonorId, cancellationToken);

            if (!donationBelongsToDonor)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(CreateRelationshipTaskCommand.DonationId), "A contribuicao informada nao pertence ao doador."),
                ]);
            }
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
            Type = SystemOptionMapper.Parse<TaskType>(request.Type),
            Priority = SystemOptionMapper.Parse<TaskPriority>(request.Priority),
            Status = RelationshipTaskStatus.Open,
            DueAtUtc = request.DueAtUtc,
        };

        _context.RelationshipTasks.Add(task);
        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = task.DonorId,
            Type = TimelineEntryType.Task,
            Title = "Tarefa criada",
            Description = task.Title,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(RelationshipTask),
            RelatedEntityId = task.Id,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
