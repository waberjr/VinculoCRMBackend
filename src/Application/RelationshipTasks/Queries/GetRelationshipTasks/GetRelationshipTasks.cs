using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.RelationshipTasks.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.RelationshipTasks.Queries.GetRelationshipTasks;

public record GetRelationshipTasksQuery : IRequest<PaginatedResult<RelationshipTaskListItemDto>>
{
    public string? Search { get; init; }
    public Guid? DonorId { get; init; }
    public string? DonorName { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? OperationalAlertId { get; init; }
    public bool? OperationalOnly { get; init; }
    public string? Status { get; init; }
    public string? Type { get; init; }
    public string? Priority { get; init; }
    public string? AssignedUserId { get; init; }
    public string? AlertSource { get; init; }
    public bool? OverdueOnly { get; init; }
    public DateTimeOffset? DueFromUtc { get; init; }
    public DateTimeOffset? DueToUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetRelationshipTasksQueryHandler : IRequestHandler<GetRelationshipTasksQuery, PaginatedResult<RelationshipTaskListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetRelationshipTasksQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedResult<RelationshipTaskListItemDto>> Handle(GetRelationshipTasksQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.RelationshipTasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(task => task.Title.ToLower().Contains(search) || (task.Donor != null && task.Donor.FullName.ToLower().Contains(search)));
        }

        if (request.DonorId is not null) query = query.Where(task => task.DonorId == request.DonorId);
        if (!string.IsNullOrWhiteSpace(request.DonorName))
        {
            var donorName = request.DonorName.Trim().ToLower();
            query = query.Where(task => task.Donor != null && task.Donor.FullName.ToLower() == donorName);
        }

        if (request.CampaignId is not null) query = query.Where(task => task.CampaignId == request.CampaignId);
        if (request.OperationalAlertId is not null) query = query.Where(task => task.OperationalAlertId == request.OperationalAlertId);
        if (request.OperationalOnly == true) query = query.Where(task => task.DonorId == null && task.OperationalAlertId != null);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = SystemOptionMapper.Parse<RelationshipTaskStatus>(request.Status);
            query = query.Where(task => task.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var priority = SystemOptionMapper.Parse<TaskPriority>(request.Priority);
            query = query.Where(task => task.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var type = SystemOptionMapper.Parse<TaskType>(request.Type);
            query = query.Where(task => task.Type == type);
        }
        if (!string.IsNullOrWhiteSpace(request.AssignedUserId)) query = query.Where(task => task.AssignedUserId == request.AssignedUserId);
        if (!string.IsNullOrWhiteSpace(request.AlertSource))
        {
            var alertSource = request.AlertSource.Trim();
            query = query.Where(task =>
                task.OperationalAlertId != null &&
                _context.OperationalAlerts.Any(alert => alert.Id == task.OperationalAlertId && alert.Source == alertSource));
        }

        if (request.OverdueOnly == true)
        {
            var now = _timeProvider.GetUtcNow();
            query = query.Where(task =>
                task.Status != RelationshipTaskStatus.Completed &&
                task.Status != RelationshipTaskStatus.Cancelled &&
                task.DueAtUtc != null &&
                task.DueAtUtc < now);
        }

        if (request.DueFromUtc is not null) query = query.Where(task => task.DueAtUtc >= request.DueFromUtc);
        if (request.DueToUtc is not null) query = query.Where(task => task.DueAtUtc <= request.DueToUtc);

        var projected = query
            .OrderBy(task => task.DueAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(task => task.Title)
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
            });

        return await PaginatedResult<RelationshipTaskListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
