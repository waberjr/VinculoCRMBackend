using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Campaigns.Queries.GetLandingPageAudit;

public sealed record GetLandingPageAuditQuery(
    string? EntityType = null,
    Guid? EntityId = null,
    string? Action = null,
    int Limit = 50) : IRequest<IReadOnlyCollection<LandingPageAuditEntryDto>>;

public sealed class GetLandingPageAuditQueryHandler : IRequestHandler<GetLandingPageAuditQuery, IReadOnlyCollection<LandingPageAuditEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public GetLandingPageAuditQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IOrganizationContext organizationContext,
        IUser user)
    {
        _context = context;
        _identityService = identityService;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<IReadOnlyCollection<LandingPageAuditEntryDto>> Handle(GetLandingPageAuditQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var query = _context.LandingPageAuditEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(entry => entry.EntityType == request.EntityType);
        }

        if (request.EntityId is not null)
        {
            query = query.Where(entry => entry.EntityId == request.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(entry => entry.Action == request.Action);
        }

        var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 200);
        var entries = await query
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Take(limit)
            .Select(entry => new LandingPageAuditEntryDto
            {
                Id = entry.Id,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                Title = entry.Title,
                Description = entry.Description,
                CreatedByUserId = entry.CreatedByUserId,
                OccurredAtUtc = entry.OccurredAtUtc,
            })
            .ToArrayAsync(cancellationToken);

        return await EnrichUsersAsync(entries, organizationId, cancellationToken);
    }

    private async Task<IReadOnlyCollection<LandingPageAuditEntryDto>> EnrichUsersAsync(
        IReadOnlyCollection<LandingPageAuditEntryDto> entries,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_user.Id) || entries.All(entry => string.IsNullOrWhiteSpace(entry.CreatedByUserId)))
        {
            return entries;
        }

        var users = await _identityService.GetAttendantsAsync(_user.Id, organizationId, cancellationToken);
        var usersById = users.ToDictionary(user => user.Id, StringComparer.OrdinalIgnoreCase);
        return entries
            .Select(entry =>
            {
                if (entry.CreatedByUserId is null || !usersById.TryGetValue(entry.CreatedByUserId, out var user))
                {
                    return entry;
                }

                return new LandingPageAuditEntryDto
                {
                    Id = entry.Id,
                    EntityType = entry.EntityType,
                    EntityId = entry.EntityId,
                    Action = entry.Action,
                    Title = entry.Title,
                    Description = entry.Description,
                    CreatedByUserId = entry.CreatedByUserId,
                    CreatedByUserName = user.DisplayName,
                    CreatedByUserEmail = user.Email,
                    OccurredAtUtc = entry.OccurredAtUtc,
                };
            })
            .ToArray();
    }
}
