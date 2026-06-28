using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;

namespace VinculoBackend.Application.Donors.Queries.GetDonorTimeline;

public record GetDonorTimelineQuery(Guid DonorId) : IRequest<DonorTimelineResponseDto?>;

public sealed class GetDonorTimelineQueryHandler : IRequestHandler<GetDonorTimelineQuery, DonorTimelineResponseDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonorTimelineQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<DonorTimelineResponseDto?> Handle(GetDonorTimelineQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            return null;
        }

        var items = await _context.DonorTimelineEntries
            .AsNoTracking()
            .Where(entry => entry.DonorId == request.DonorId)
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Select(entry => new DonorTimelineEntryDto
            {
                Id = entry.Id,
                DonorName = entry.Donor.FullName,
                Title = entry.Title,
                Description = entry.Description ?? string.Empty,
                OccurredAt = entry.OccurredAtUtc,
                Tone = entry.TypeOption.Code == "Donation"
                    ? "green"
                    : entry.TypeOption.Code == "Task"
                        ? "blue"
                        : entry.TypeOption.Code == "Contact"
                            ? "yellow"
                            : "neutral",
            })
            .ToListAsync(cancellationToken);

        return new DonorTimelineResponseDto { Items = items };
    }
}
