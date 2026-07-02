using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.RelationshipTasks.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.RelationshipTasks.Queries.GetRelationshipTasks;

public record GetRelationshipTasksQuery : IRequest<PaginatedResult<RelationshipTaskListItemDto>>
{
    public string? Search { get; init; }
    public Guid? DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public string? AssignedUserId { get; init; }
    public DateTimeOffset? DueFromUtc { get; init; }
    public DateTimeOffset? DueToUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetRelationshipTasksQueryHandler : IRequestHandler<GetRelationshipTasksQuery, PaginatedResult<RelationshipTaskListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetRelationshipTasksQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<RelationshipTaskListItemDto>> Handle(GetRelationshipTasksQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.RelationshipTasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(task => task.Title.ToLower().Contains(search) || task.Donor.FullName.ToLower().Contains(search));
        }

        if (request.DonorId is not null) query = query.Where(task => task.DonorId == request.DonorId);
        if (request.CampaignId is not null) query = query.Where(task => task.CampaignId == request.CampaignId);
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
        if (!string.IsNullOrWhiteSpace(request.AssignedUserId)) query = query.Where(task => task.AssignedUserId == request.AssignedUserId);
        if (request.DueFromUtc is not null) query = query.Where(task => task.DueAtUtc >= request.DueFromUtc);
        if (request.DueToUtc is not null) query = query.Where(task => task.DueAtUtc <= request.DueToUtc);

        var projected = query
            .OrderBy(task => task.DueAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(task => task.Title)
            .Select(task => new RelationshipTaskListItemDto
            {
                Id = task.Id,
                DonorId = task.DonorId,
                DonorName = task.Donor.FullName,
                CampaignId = task.CampaignId,
                CampaignName = task.Campaign == null ? null : task.Campaign.Name,
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
