using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.OperationalAlerts.Models;
using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertAudit;
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
        return new OperationalAlertDetailDto
        {
            Alert = Copy(alert, assignedUserName),
            AuditEntries = auditEntries,
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
    };
}
