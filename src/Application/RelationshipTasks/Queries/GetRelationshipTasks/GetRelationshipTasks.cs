using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.RelationshipTasks.Models;

namespace VinculoBackend.Application.RelationshipTasks.Queries.GetRelationshipTasks;

public record GetRelationshipTasksQuery : IRequest<PaginatedResult<RelationshipTaskListItemDto>>
{
    public string? Search { get; init; }
    public Guid? DonorId { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? StatusOptionId { get; init; }
    public Guid? PriorityOptionId { get; init; }
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
        if (request.StatusOptionId is not null) query = query.Where(task => task.StatusOptionId == request.StatusOptionId);
        if (request.PriorityOptionId is not null) query = query.Where(task => task.PriorityOptionId == request.PriorityOptionId);
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
                Type = new OptionDto { Id = task.TypeOption.Id, Category = task.TypeOption.Category, Code = task.TypeOption.Code, Name = task.TypeOption.Name, Color = task.TypeOption.Color, SortOrder = task.TypeOption.SortOrder, IsSystem = task.TypeOption.IsSystem, IsActive = task.TypeOption.IsActive },
                Priority = new OptionDto { Id = task.PriorityOption.Id, Category = task.PriorityOption.Category, Code = task.PriorityOption.Code, Name = task.PriorityOption.Name, Color = task.PriorityOption.Color, SortOrder = task.PriorityOption.SortOrder, IsSystem = task.PriorityOption.IsSystem, IsActive = task.PriorityOption.IsActive },
                Status = new OptionDto { Id = task.StatusOption.Id, Category = task.StatusOption.Category, Code = task.StatusOption.Code, Name = task.StatusOption.Name, Color = task.StatusOption.Color, SortOrder = task.StatusOption.SortOrder, IsSystem = task.StatusOption.IsSystem, IsActive = task.StatusOption.IsActive },
                ContactOutcome = task.ContactOutcomeOption == null ? null : new OptionDto { Id = task.ContactOutcomeOption.Id, Category = task.ContactOutcomeOption.Category, Code = task.ContactOutcomeOption.Code, Name = task.ContactOutcomeOption.Name, Color = task.ContactOutcomeOption.Color, SortOrder = task.ContactOutcomeOption.SortOrder, IsSystem = task.ContactOutcomeOption.IsSystem, IsActive = task.ContactOutcomeOption.IsActive },
            });

        return await PaginatedResult<RelationshipTaskListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
