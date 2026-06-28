using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DonorTags.Models;

namespace VinculoBackend.Application.DonorTags.Queries.GetDonorTags;

public record GetDonorTagsQuery(bool IncludeInactive = false) : IRequest<IReadOnlyCollection<DonorTagDto>>;

public sealed class GetDonorTagsQueryHandler : IRequestHandler<GetDonorTagsQuery, IReadOnlyCollection<DonorTagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonorTagsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<DonorTagDto>> Handle(GetDonorTagsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.DonorTags.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(tag => tag.IsActive);
        }

        return await query
            .OrderBy(tag => tag.Name)
            .Select(tag => new DonorTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                IsActive = tag.IsActive,
            })
            .ToListAsync(cancellationToken);
    }
}
