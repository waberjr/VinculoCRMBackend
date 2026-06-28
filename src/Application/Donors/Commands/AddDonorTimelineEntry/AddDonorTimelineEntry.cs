using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donors.Commands.AddDonorTimelineEntry;

public record AddDonorTimelineEntryCommand(Guid DonorId, string Title, string? Description, string Type = "Note") : IRequest<Guid>;

public sealed class AddDonorTimelineEntryCommandHandler : IRequestHandler<AddDonorTimelineEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public AddDonorTimelineEntryCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
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
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", request.Type, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
        };

        _context.DonorTimelineEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
