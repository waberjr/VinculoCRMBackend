using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertAudit;
using VinculoBackend.Application.RelationshipTasks.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertDetail;

public sealed record GetOperationalAlertDetailQuery(Guid Id) : IRequest<OperationalAlertDetailDto>;

public sealed class GetOperationalAlertDetailQueryHandler : IRequestHandler<GetOperationalAlertDetailQuery, OperationalAlertDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISender _sender;

    public GetOperationalAlertDetailQueryHandler(IApplicationDbContext context, IIdentityService identityService, ISender sender)
    {
        _context = context;
        _identityService = identityService;
        _sender = sender;
    }

    public async Task<OperationalAlertDetailDto> Handle(GetOperationalAlertDetailQuery request, CancellationToken cancellationToken)
    {
        var alert = await _context.OperationalAlerts
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new OperationalAlertDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Severity = entity.Severity,
                Status = entity.Status,
                Source = entity.Source,
                RelatedEntityType = entity.RelatedEntityType,
                RelatedEntityId = entity.RelatedEntityId,
                ActionUrl = entity.ActionUrl,
                AssignedUserId = entity.AssignedUserId,
                DueAtUtc = entity.DueAtUtc,
                OccurredAtUtc = entity.OccurredAtUtc,
                AcknowledgedAtUtc = entity.AcknowledgedAtUtc,
                ResolvedAtUtc = entity.ResolvedAtUtc,
                ResolutionNote = entity.ResolutionNote,
                OpenTasksCount = _context.RelationshipTasks.Count(task =>
                    task.OperationalAlertId == entity.Id &&
                    task.Status == Domain.Enums.RelationshipTaskStatus.Open),
                InProgressTasksCount = _context.RelationshipTasks.Count(task =>
                    task.OperationalAlertId == entity.Id &&
                    task.Status == Domain.Enums.RelationshipTaskStatus.InProgress),
                CompletedTasksCount = _context.RelationshipTasks.Count(task =>
                    task.OperationalAlertId == entity.Id &&
                    task.Status == Domain.Enums.RelationshipTaskStatus.Completed),
                LastCompletedTaskTitle = _context.RelationshipTasks
                    .Where(task => task.OperationalAlertId == entity.Id && task.Status == Domain.Enums.RelationshipTaskStatus.Completed)
                    .OrderByDescending(task => task.CompletedAtUtc)
                    .Select(task => task.Title)
                    .FirstOrDefault(),
                LastCompletedTaskCompletedAtUtc = _context.RelationshipTasks
                    .Where(task => task.OperationalAlertId == entity.Id && task.Status == Domain.Enums.RelationshipTaskStatus.Completed)
                    .OrderByDescending(task => task.CompletedAtUtc)
                    .Select(task => task.CompletedAtUtc)
                    .FirstOrDefault(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (alert is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(OperationalAlert), request.Id.ToString());
        }

        var assignedUserName = string.IsNullOrWhiteSpace(alert.AssignedUserId)
            ? null
            : await _identityService.GetUserNameAsync(alert.AssignedUserId);
        var auditEntries = await _sender.Send(new GetOperationalAlertAuditQuery(request.Id), cancellationToken);
        var tasks = await _context.RelationshipTasks
            .AsNoTracking()
            .Where(task => task.OperationalAlertId == request.Id)
            .OrderBy(task => task.DueAtUtc ?? DateTimeOffset.MaxValue)
            .Select(task => new RelationshipTaskListItemDto
            {
                Id = task.Id,
                DonorId = task.DonorId,
                DonorName = task.Donor == null ? null : task.Donor.FullName,
                CampaignId = task.CampaignId,
                CampaignName = task.Campaign == null ? null : task.Campaign.Name,
                OperationalAlertId = task.OperationalAlertId,
                Title = task.Title,
                Description = task.Description,
                AssignedUserId = task.AssignedUserId,
                DueAtUtc = task.DueAtUtc,
                CompletedAtUtc = task.CompletedAtUtc,
                Type = SystemOptionMapper.ToOptionDto(task.Type),
                Priority = SystemOptionMapper.ToOptionDto(task.Priority),
                Status = SystemOptionMapper.ToOptionDto(task.Status),
                ContactOutcome = task.ContactOutcome == null ? null : SystemOptionMapper.ToOptionDto(task.ContactOutcome.Value),
            })
            .ToArrayAsync(cancellationToken);
        return new OperationalAlertDetailDto
        {
            Alert = Copy(alert, assignedUserName),
            AuditEntries = auditEntries,
            Tasks = tasks,
        };
    }

    private static OperationalAlertDto Copy(OperationalAlertDto alert, string? assignedUserName) => new()
    {
        Id = alert.Id,
        Title = alert.Title,
        Description = alert.Description,
        Severity = alert.Severity,
        Status = alert.Status,
        Source = alert.Source,
        RelatedEntityType = alert.RelatedEntityType,
        RelatedEntityId = alert.RelatedEntityId,
        ActionUrl = alert.ActionUrl,
        AssignedUserId = alert.AssignedUserId,
        AssignedUserName = assignedUserName,
        DueAtUtc = alert.DueAtUtc,
        OccurredAtUtc = alert.OccurredAtUtc,
        AcknowledgedAtUtc = alert.AcknowledgedAtUtc,
        ResolvedAtUtc = alert.ResolvedAtUtc,
        ResolutionNote = alert.ResolutionNote,
        OpenTasksCount = alert.OpenTasksCount,
        InProgressTasksCount = alert.InProgressTasksCount,
        CompletedTasksCount = alert.CompletedTasksCount,
        LastCompletedTaskTitle = alert.LastCompletedTaskTitle,
        LastCompletedTaskCompletedAtUtc = alert.LastCompletedTaskCompletedAtUtc,
    };
}
