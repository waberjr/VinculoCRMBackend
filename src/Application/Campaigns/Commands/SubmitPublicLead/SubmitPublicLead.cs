using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Commands.SubmitPublicLead;

public sealed record SubmitPublicLeadCommand : IRequest<PublicLeadSubmissionDto>
{
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public bool AllowsCommunication { get; init; } = true;
    public decimal? DonationAmount { get; init; }
    public string? Source { get; init; }
    public string? UtmSource { get; init; }
    public string? UtmMedium { get; init; }
    public string? UtmCampaign { get; init; }
    public string? UtmContent { get; init; }
    public string? UtmTerm { get; init; }
}

public sealed class SubmitPublicLeadCommandHandler : IRequestHandler<SubmitPublicLeadCommand, PublicLeadSubmissionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SubmitPublicLeadCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PublicLeadSubmissionDto> Handle(SubmitPublicLeadCommand request, CancellationToken cancellationToken)
    {
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        var target = await TargetOrganization(targetType, request.TargetId, cancellationToken);
        if (target is null)
        {
            throw new Common.Exceptions.NotFoundException(targetType, request.TargetId.ToString());
        }

        var email = TrimToNull(request.Email);
        var phone = TrimToNull(request.Phone);
        var sourceDescription = SourceDescription(request);
        var targetOrganizationId = target.Value.OrganizationId;
        var targetName = target.Value.Name;
        var donor = await _context.Donors
            .FirstOrDefaultAsync(entity =>
                entity.OrganizationId == targetOrganizationId &&
                ((email != null && entity.Email == email) || (phone != null && entity.Phone == phone)), cancellationToken);

        var created = false;
        if (donor is null)
        {
            donor = Donor.Create(
                targetOrganizationId,
                request.FullName,
                DonorPersonType.Individual,
                DonorStatus.Lead,
                null,
                null,
                null,
                null,
                email,
                phone,
                phone,
                null,
                null,
                null,
                null,
                null,
                null,
                request.AllowsCommunication,
                false,
                null,
                null,
                targetType == "campaign" ? request.TargetId : null,
                $"Lead gerado pela landing page de {targetType}. {sourceDescription}");
            donor.ReplaceEmails(email is null ? [] : [DonorEmail.Create(targetOrganizationId, donor.Id, EmailType.Personal, email, true)]);
            donor.ReplacePhones(phone is null ? [] : [DonorPhone.Create(targetOrganizationId, donor.Id, PhoneType.WhatsApp, phone, true)]);
            _context.Donors.Add(donor);
            created = true;
        }

        var donationId = CreatePendingDonationIfRequested(request, targetType, targetOrganizationId, donor.Id);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = targetOrganizationId,
            DonorId = donor.Id,
            Type = TimelineEntryType.Note,
            Title = "Interesse pela landing page",
            Description = LandingTimelineDescription(targetName, request.DonationAmount, sourceDescription),
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            RelatedEntityType = targetType,
            RelatedEntityId = request.TargetId,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new PublicLeadSubmissionDto { DonorId = donor.Id, DonationId = donationId, Created = created };
    }

    private Guid? CreatePendingDonationIfRequested(SubmitPublicLeadCommand request, string targetType, Guid organizationId, Guid donorId)
    {
        if (request.DonationAmount is null || request.DonationAmount <= 0)
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        var donation = Donation.Create(
            organizationId,
            donorId,
            targetType == "campaign" ? request.TargetId : null,
            null,
            request.DonationAmount.Value,
            DonationType.Pledge,
            DonationStatus.Pending,
            PaymentMethod.Pix,
            now.AddDays(7),
            null,
            null,
            null,
            "Promessa gerada por formulário público de landing.",
            null);

        _context.Donations.Add(donation);
        if (targetType == "project")
        {
            _context.DonationProjects.Add(new DonationProject
            {
                OrganizationId = organizationId,
                DonationId = donation.Id,
                ProjectId = request.TargetId,
            });
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donorId,
            Type = TimelineEntryType.Donation,
            Title = "Promessa de contribuicao criada",
            Description = $"Valor: {request.DonationAmount.Value:C}. Origem: landing publica.",
            OccurredAtUtc = now,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        return donation.Id;
    }

    private async Task<(Guid OrganizationId, string Name)?> TargetOrganization(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        if (targetType == "campaign")
        {
            var campaign = await _context.Campaigns
                .IgnoreQueryFilters()
                .Where(campaign => !campaign.IsDeleted && campaign.Id == targetId)
                .Select(campaign => new { campaign.OrganizationId, campaign.Name })
                .FirstOrDefaultAsync(cancellationToken);
            return campaign is null ? null : (campaign.OrganizationId, campaign.Name);
        }

        if (targetType == "project")
        {
            var project = await _context.Projects
                .IgnoreQueryFilters()
                .Where(project => !project.IsDeleted && project.Id == targetId)
                .Select(project => new { project.OrganizationId, project.Name })
                .FirstOrDefaultAsync(cancellationToken);
            return project is null ? null : (project.OrganizationId, project.Name);
        }

        return null;
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string LandingTimelineDescription(string targetName, decimal? amount, string sourceDescription)
    {
        var parts = new List<string> { targetName };
        if (amount is > 0)
        {
            parts.Add($"Interesse em contribuir com {amount.Value:C}.");
        }

        if (!string.IsNullOrWhiteSpace(sourceDescription))
        {
            parts.Add(sourceDescription);
        }

        return string.Join(" ", parts);
    }

    private static string SourceDescription(SubmitPublicLeadCommand request)
    {
        var parts = new List<string>();
        AddPart(parts, "Fonte", request.Source);
        AddPart(parts, "utm_source", request.UtmSource);
        AddPart(parts, "utm_medium", request.UtmMedium);
        AddPart(parts, "utm_campaign", request.UtmCampaign);
        AddPart(parts, "utm_content", request.UtmContent);
        AddPart(parts, "utm_term", request.UtmTerm);
        return string.Join(" | ", parts);
    }

    private static void AddPart(ICollection<string> parts, string label, string? value)
    {
        var trimmed = TrimToNull(value);
        if (trimmed is not null)
        {
            parts.Add($"{label}: {trimmed}");
        }
    }
}
