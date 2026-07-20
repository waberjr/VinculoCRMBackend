using FluentValidation.Results;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateTasksFromSegment;

public sealed record CreateTasksFromSegmentCommand : IRequest<CreateTasksFromSegmentResultDto>
{
    public string Segment { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = "Call";
    public string Priority { get; init; } = "Medium";
    public DateTimeOffset? DueAtUtc { get; init; }
    public string? AssignedUserId { get; init; }
    public int Limit { get; init; } = 100;
}

public sealed class CreateTasksFromSegmentResultDto
{
    public int CreatedCount { get; init; }
    public int SkippedBlockedCount { get; init; }
}

public sealed class CreateTasksFromSegmentCommandHandler : IRequestHandler<CreateTasksFromSegmentCommand, CreateTasksFromSegmentResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CreateTasksFromSegmentCommandHandler(
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

    public async Task<CreateTasksFromSegmentResultDto> Handle(CreateTasksFromSegmentCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        if (string.IsNullOrWhiteSpace(request.Segment))
        {
            throw new Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.Segment), "Informe o segmento.")]);
        }

        var donors = await ApplySegment(_context.Donors.AsNoTracking(), request.Segment)
            .OrderBy(donor => donor.FullName)
            .Take(Math.Clamp(request.Limit, 1, 500))
            .Select(donor => new
            {
                donor.Id,
                donor.AllowsCommunication,
                donor.DoNotContact,
            })
            .ToListAsync(cancellationToken);

        var now = _timeProvider.GetUtcNow();
        var created = 0;
        var skipped = 0;
        foreach (var donor in donors)
        {
            if (donor.DoNotContact || !donor.AllowsCommunication)
            {
                skipped++;
                continue;
            }

            var task = RelationshipTask.Create(
                organizationId,
                donor.Id,
                null,
                null,
                null,
                request.Title,
                request.Description,
                request.AssignedUserId,
                _user.Id,
                SystemOptionMapper.Parse<TaskType>(request.Type),
                SystemOptionMapper.Parse<TaskPriority>(request.Priority),
                request.DueAtUtc);

            _context.RelationshipTasks.Add(task);
            _context.DonorTimelineEntries.Add(new DonorTimelineEntry
            {
                OrganizationId = organizationId,
                DonorId = donor.Id,
                Type = TimelineEntryType.Task,
                Title = "Tarefa criada por segmento",
                Description = task.Title,
                OccurredAtUtc = now,
                CreatedByUserId = _user.Id,
                RelatedEntityType = nameof(RelationshipTask),
                RelatedEntityId = task.Id,
            });
            created++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new CreateTasksFromSegmentResultDto { CreatedCount = created, SkippedBlockedCount = skipped };
    }

    private IQueryable<Donor> ApplySegment(IQueryable<Donor> query, string segment)
    {
        var now = _timeProvider.GetUtcNow();
        var staleDonationStartUtc = now.AddDays(-90);
        var staleContactStartUtc = now.AddDays(-30);
        var newDonorsStartUtc = now.AddDays(-30);

        return segment switch
        {
            "Inactive" => query.Where(donor => donor.Status == DonorStatus.Inactive),
            "AtRisk" => query.Where(donor => donor.Status == DonorStatus.AtRisk),
            "OverdueDonations" => query.Where(donor => _context.Donations.Any(donation =>
                donation.DonorId == donor.Id &&
                (donation.Status == DonationStatus.Overdue ||
                    (donation.Status == DonationStatus.Pending && donation.ExpectedAtUtc < now)))),
            "LeadsWithoutDonation" => query.Where(donor =>
                donor.Status == DonorStatus.Lead &&
                !_context.Donations.Any(donation => donation.DonorId == donor.Id && donation.Status == DonationStatus.Confirmed && donation.PaidAtUtc != null)),
            "NoDonation90Days" => query.Where(donor =>
                _context.Donations.Any(donation => donation.DonorId == donor.Id && donation.Status == DonationStatus.Confirmed && donation.PaidAtUtc != null) &&
                !_context.Donations.Any(donation => donation.DonorId == donor.Id && donation.Status == DonationStatus.Confirmed && donation.PaidAtUtc != null && donation.PaidAtUtc >= staleDonationStartUtc)),
            "InterruptedRecurring" => query.Where(donor => _context.DonationPlans.Any(plan =>
                plan.DonorId == donor.Id &&
                (plan.Status == DonationPlanStatus.Paused || plan.Status == DonationPlanStatus.Cancelled))),
            "NoRecentContact" => query.Where(donor => !_context.DonorTimelineEntries.Any(entry =>
                entry.DonorId == donor.Id &&
                entry.Type == TimelineEntryType.Contact &&
                entry.OccurredAtUtc >= staleContactStartUtc)),
            "NewDonors" => query.Where(donor => donor.Created >= newDonorsStartUtc),
            _ => query.Where(_ => false),
        };
    }
}
