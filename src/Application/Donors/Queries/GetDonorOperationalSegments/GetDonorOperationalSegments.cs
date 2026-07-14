using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donors.Queries.GetDonorOperationalSegments;

public sealed record GetDonorOperationalSegmentsQuery : IRequest<IReadOnlyCollection<DonorOperationalSegmentDto>>;

public sealed class GetDonorOperationalSegmentsQueryHandler : IRequestHandler<GetDonorOperationalSegmentsQuery, IReadOnlyCollection<DonorOperationalSegmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetDonorOperationalSegmentsQueryHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<DonorOperationalSegmentDto>> Handle(GetDonorOperationalSegmentsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var now = _timeProvider.GetUtcNow();
        var newDonorsStartUtc = now.AddDays(-30);
        var staleContactStartUtc = now.AddDays(-30);
        var staleDonationStartUtc = now.AddDays(-90);

        var inactive = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => donor.Status == DonorStatus.Inactive, cancellationToken);
        var atRisk = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => donor.Status == DonorStatus.AtRisk, cancellationToken);
        var overdueDonations = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => _context.Donations.Any(donation =>
                donation.DonorId == donor.Id &&
                (donation.Status == DonationStatus.Overdue ||
                    (donation.Status == DonationStatus.Pending && donation.ExpectedAtUtc < now))), cancellationToken);
        var noRecentContact = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => !_context.DonorTimelineEntries.Any(entry =>
                entry.DonorId == donor.Id &&
                entry.Type == TimelineEntryType.Contact &&
                entry.OccurredAtUtc >= staleContactStartUtc), cancellationToken);
        var leadsWithoutDonation = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor =>
                donor.Status == DonorStatus.Lead &&
                !_context.Donations.Any(donation =>
                    donation.DonorId == donor.Id &&
                    donation.Status == DonationStatus.Confirmed &&
                    donation.PaidAtUtc != null), cancellationToken);
        var noDonation90Days = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor =>
                _context.Donations.Any(donation =>
                    donation.DonorId == donor.Id &&
                    donation.Status == DonationStatus.Confirmed &&
                    donation.PaidAtUtc != null) &&
                !_context.Donations.Any(donation =>
                    donation.DonorId == donor.Id &&
                    donation.Status == DonationStatus.Confirmed &&
                    donation.PaidAtUtc != null &&
                    donation.PaidAtUtc >= staleDonationStartUtc), cancellationToken);
        var interruptedRecurring = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => _context.DonationPlans.Any(plan =>
                plan.DonorId == donor.Id &&
                (plan.Status == DonationPlanStatus.Paused || plan.Status == DonationPlanStatus.Cancelled)), cancellationToken);
        var newDonors = await _context.Donors
            .AsNoTracking()
            .CountAsync(donor => donor.Created >= newDonorsStartUtc, cancellationToken);

        return
        [
            Segment("AtRisk", "Em risco", "Doadores marcados para retencao.", atRisk, "yellow"),
            Segment("OverdueDonations", "Cobrancas vencidas", "Doadores com contribuicoes pendentes vencidas.", overdueDonations, "red"),
            Segment("NoRecentContact", "Sem contato recente", "Sem registro de contato nos ultimos 30 dias.", noRecentContact, "blue"),
            Segment("NoDonation90Days", "Sem doacao ha 90 dias", "Doadores confirmados sem nova doacao recente.", noDonation90Days, "yellow"),
            Segment("InterruptedRecurring", "Recorrencia interrompida", "Doadores com plano pausado ou cancelado.", interruptedRecurring, "red"),
            Segment("LeadsWithoutDonation", "Leads sem conversao", "Leads ainda sem doacao confirmada.", leadsWithoutDonation, "blue"),
            Segment("Inactive", "Inativos", "Doadores inativos para reativacao.", inactive, "neutral"),
            Segment("NewDonors", "Novos doadores", "Cadastros criados nos ultimos 30 dias.", newDonors, "green"),
        ];
    }

    private static DonorOperationalSegmentDto Segment(string code, string name, string description, int count, string tone)
    {
        return new DonorOperationalSegmentDto
        {
            Code = code,
            Name = name,
            Description = description,
            Count = count,
            Tone = tone,
        };
    }
}
