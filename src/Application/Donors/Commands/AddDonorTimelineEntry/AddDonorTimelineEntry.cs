using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donors.Commands.AddDonorTimelineEntry;

public record AddDonorTimelineEntryCommand(Guid DonorId, string Title, string? Description, string Type = "Note") : IRequest<Guid>;

public sealed class AddDonorTimelineEntryCommandHandler : IRequestHandler<AddDonorTimelineEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AddDonorTimelineEntryCommandHandler(
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

    public async Task<Guid> Handle(AddDonorTimelineEntryCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donorExists = await _context.Donors.AsNoTracking().AnyAsync(donor => donor.Id == request.DonorId, cancellationToken);
        if (!donorExists)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.DonorId.ToString());
        }

        var entry = new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = request.DonorId,
            Type = SystemOptionMapper.Parse<TimelineEntryType>(request.Type),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            CreatedByUserId = _user.Id,
        };

        _context.DonorTimelineEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
