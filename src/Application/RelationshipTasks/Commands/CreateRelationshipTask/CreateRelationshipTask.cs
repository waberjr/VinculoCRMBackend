using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using VinculoBackend.Application.OperationalAlerts.Services;
using FluentValidation.Results;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;

public record CreateRelationshipTaskCommand : IRequest<Guid>
{
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

public sealed class CreateRelationshipTaskCommandHandler : IRequestHandler<CreateRelationshipTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CreateRelationshipTaskCommandHandler(
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

    public async Task<Guid> Handle(CreateRelationshipTaskCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        Donor? donor = null;
        if (request.DonorId is not null)
        {
            donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == request.DonorId, cancellationToken);
            if (donor is null)
            {
                throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.Value.ToString());
            }
        }

        if (donor is not null && (donor.DoNotContact || !donor.AllowsCommunication))
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
                .AnyAsync(donation => donation.Id == request.DonationId && request.DonorId != null && donation.DonorId == request.DonorId, cancellationToken);

            if (!donationBelongsToDonor)
            {
                throw new Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(nameof(CreateRelationshipTaskCommand.DonationId), "A contribuição informada não pertence ao doador."),
                ]);
            }
        }

        OperationalAlert? alert = null;
        if (request.OperationalAlertId is not null)
        {
            alert = await _context.OperationalAlerts.FirstOrDefaultAsync(entity => entity.Id == request.OperationalAlertId, cancellationToken);
            if (alert is null)
            {
                throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.OperationalAlertId.Value.ToString());
            }
        }

        var task = RelationshipTask.Create(
            organizationId,
            request.DonorId,
            request.CampaignId,
            request.DonationId,
            request.OperationalAlertId,
            request.Title,
            request.Description,
            request.AssignedUserId,
            _user.Id,
            SystemOptionMapper.Parse<TaskType>(request.Type),
            SystemOptionMapper.Parse<TaskPriority>(request.Priority),
            request.DueAtUtc);

        _context.RelationshipTasks.Add(task);
        if (task.DonorId is not null)
        {
            _context.DonorTimelineEntries.Add(new DonorTimelineEntry
            {
                OrganizationId = organizationId,
                DonorId = task.DonorId.Value,
                Type = TimelineEntryType.Task,
                Title = "Tarefa criada",
                Description = task.Title,
                OccurredAtUtc = _timeProvider.GetUtcNow(),
                CreatedByUserId = _user.Id,
                RelatedEntityType = nameof(RelationshipTask),
                RelatedEntityId = task.Id,
            });
        }

        if (alert is not null)
        {
            _context.OperationalAlertAuditEntries.Add(OperationalAlertAudit.Create(
                alert,
                "TaskCreated",
                "Tarefa criada a partir do alerta",
                $"Tarefa {task.Id}: {task.Title}",
                _timeProvider.GetUtcNow(),
                _user.Id));
        }
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
