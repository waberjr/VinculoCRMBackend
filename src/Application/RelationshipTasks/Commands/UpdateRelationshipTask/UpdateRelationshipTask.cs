using FluentValidation.Results;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.RelationshipTasks.Commands.UpdateRelationshipTask;

public record UpdateRelationshipTaskCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid? DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? DonationId { get; init; }
    public Guid? OperationalAlertId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AssignedUserId { get; init; }
    public string Type { get; init; } = "Call";
    public string Priority { get; init; } = "Medium";
    public DateTimeOffset? DueAtUtc { get; init; }
    public bool ConfirmBlockedContact { get; init; }
    public string? BlockedContactJustification { get; init; }
}

public sealed class UpdateRelationshipTaskCommandHandler : IRequestHandler<UpdateRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public UpdateRelationshipTaskCommandHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        Donor? donor = null;
        if (request.DonorId is not null)
        {
            donor = await _context.Donors.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == request.DonorId, cancellationToken);
            if (donor is null)
            {
                throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.Value.ToString());
            }
        }

        if (donor is not null &&
            (donor.DoNotContact || !donor.AllowsCommunication) &&
            (!request.ConfirmBlockedContact || string.IsNullOrWhiteSpace(request.BlockedContactJustification)))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateRelationshipTaskCommand.DonorId), "Doador bloqueado para contato exige confirmacao explicita e justificativa."),
            ]);
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
                .AnyAsync(donation => donation.Id == request.DonationId && request.DonorId != null && donation.DonorId == request.DonorId, cancellationToken);

            if (!donationBelongsToDonor)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(UpdateRelationshipTaskCommand.DonationId), "A contribuicao informada nao pertence ao doador."),
                ]);
            }
        }

        task.Update(
            request.DonorId,
            request.CampaignId,
            request.DonationId,
            request.OperationalAlertId,
            request.Title,
            request.Description,
            request.AssignedUserId,
            SystemOptionMapper.Parse<TaskType>(request.Type),
            SystemOptionMapper.Parse<TaskPriority>(request.Priority),
            request.DueAtUtc);

        if (task.DonorId is not null)
        {
            _context.DonorTimelineEntries.Add(new DonorTimelineEntry
            {
                OrganizationId = task.OrganizationId,
                DonorId = task.DonorId.Value,
                Type = TimelineEntryType.Task,
                Title = "Tarefa atualizada",
                Description = task.Title,
                OccurredAtUtc = _timeProvider.GetUtcNow(),
                CreatedByUserId = _user.Id,
                RelatedEntityType = nameof(RelationshipTask),
                RelatedEntityId = task.Id,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
