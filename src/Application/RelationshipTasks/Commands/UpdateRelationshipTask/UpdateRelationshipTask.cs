using FluentValidation.Results;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.RelationshipTasks.Commands.UpdateRelationshipTask;

public record UpdateRelationshipTaskCommand : IRequest
{
    public Guid Id { get; init; }
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

public sealed class UpdateRelationshipTaskCommandHandler : IRequestHandler<UpdateRelationshipTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public UpdateRelationshipTaskCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task Handle(UpdateRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var task = await _context.RelationshipTasks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (task is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(RelationshipTask), request.Id.ToString());
        }

        if (task.Status is RelationshipTaskStatus.Completed or RelationshipTaskStatus.Cancelled)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateRelationshipTaskCommand.Id), "Tarefas concluidas ou canceladas nao podem ser atualizadas."),
            ]);
        }

        var donor = await _context.Donors.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == request.DonorId, cancellationToken);
        if (donor is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        if ((donor.DoNotContact || !donor.AllowsCommunication) &&
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
                .AnyAsync(donation => donation.Id == request.DonationId && donation.DonorId == request.DonorId, cancellationToken);

            if (!donationBelongsToDonor)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(UpdateRelationshipTaskCommand.DonationId), "A contribuicao informada nao pertence ao doador."),
                ]);
            }
        }

        task.DonorId = request.DonorId;
        task.CampaignId = request.CampaignId;
        task.DonationId = request.DonationId;
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.AssignedUserId = request.AssignedUserId;
        task.Type = SystemOptionMapper.Parse<TaskType>(request.Type);
        task.Priority = SystemOptionMapper.Parse<TaskPriority>(request.Priority);
        task.DueAtUtc = request.DueAtUtc;

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = task.OrganizationId,
            DonorId = task.DonorId,
            Type = TimelineEntryType.Task,
            Title = "Tarefa atualizada",
            Description = task.Title,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = nameof(RelationshipTask),
            RelatedEntityId = task.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
