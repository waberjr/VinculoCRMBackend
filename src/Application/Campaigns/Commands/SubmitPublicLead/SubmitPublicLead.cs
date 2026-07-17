using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Services;
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
    public string? Website { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public IReadOnlyDictionary<string, string> CustomFields { get; init; } = new Dictionary<string, string>();
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

        var now = _timeProvider.GetUtcNow();
        var rateLimit = await RateLimit(target.Value.OrganizationId, targetType, request.TargetId, cancellationToken);
        var source = TrimToNull(request.Source) ?? TrimToNull(request.UtmSource) ?? "landing";
        var fingerprintHash = LandingPageViewDeduplication.Fingerprint(
            targetType,
            request.TargetId,
            source,
            TrimToNull(request.IpAddress) ?? "unknown-ip",
            TrimToNull(request.UserAgent) ?? "unknown-agent");
        var blockedReason = await BlockedReason(request, target.Value.OrganizationId, targetType, fingerprintHash, rateLimit, now, cancellationToken);
        if (blockedReason is not null)
        {
            _context.LandingPageSubmissionAttempts.Add(SubmissionAttempt(target.Value.OrganizationId, targetType, request.TargetId, fingerprintHash, source, true, blockedReason, now));
            await AddHighProtectionAlertIfNeeded(target.Value.OrganizationId, targetType, request.TargetId, target.Value.Name, now, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            throw new Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure(nameof(SubmitPublicLeadCommand.Website), "Solicitacao rejeitada.")]);
        }

        _context.LandingPageSubmissionAttempts.Add(SubmissionAttempt(target.Value.OrganizationId, targetType, request.TargetId, fingerprintHash, source, false, null, now));

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
            Description = LandingTimelineDescription(targetName, request.DonationAmount, sourceDescription, request.CustomFields),
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            RelatedEntityType = targetType,
            RelatedEntityId = request.TargetId,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new PublicLeadSubmissionDto { DonorId = donor.Id, DonationId = donationId, Created = created };
    }

    private async Task<string?> BlockedReason(
        SubmitPublicLeadCommand request,
        Guid organizationId,
        string targetType,
        string fingerprintHash,
        (int WindowMinutes, int MaxAttempts) rateLimit,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return "honeypot";
        }

        var source = TrimToNull(request.Source) ?? TrimToNull(request.UtmSource) ?? "landing";
        var manuallyBlocked = await _context.LandingPageBlockRules
            .IgnoreQueryFilters()
            .AnyAsync(rule =>
                !rule.IsDeleted &&
                rule.OrganizationId == organizationId &&
                rule.IsActive &&
                (rule.ExpiresAtUtc == null || rule.ExpiresAtUtc > now) &&
                rule.TargetType == targetType &&
                rule.TargetId == request.TargetId &&
                ((rule.FingerprintHash != null && rule.FingerprintHash == fingerprintHash) ||
                 (rule.Source != null && rule.Source == source)),
                cancellationToken);
        if (manuallyBlocked)
        {
            return "manual-block-rule";
        }

        var windowStart = now.AddMinutes(-rateLimit.WindowMinutes);
        var attempts = await _context.LandingPageSubmissionAttempts
            .IgnoreQueryFilters()
            .CountAsync(attempt =>
                !attempt.IsDeleted &&
                attempt.OrganizationId == organizationId &&
                attempt.TargetType == targetType &&
                attempt.TargetId == request.TargetId &&
                attempt.FingerprintHash == fingerprintHash &&
                attempt.AttemptedAtUtc >= windowStart,
                cancellationToken);

        return attempts >= rateLimit.MaxAttempts ? "fingerprint-rate-limit" : null;
    }

    private async Task<(int WindowMinutes, int MaxAttempts)> RateLimit(Guid organizationId, string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var page = await _context.LandingPages
            .IgnoreQueryFilters()
            .Where(page =>
                !page.IsDeleted &&
                page.OrganizationId == organizationId &&
                page.TargetType == targetType &&
                page.TargetId == targetId)
            .Select(page => new
            {
                page.SubmissionLimitWindowMinutes,
                page.SubmissionLimitMaxAttempts,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return page is null ? (15, 5) : (page.SubmissionLimitWindowMinutes, page.SubmissionLimitMaxAttempts);
    }

    private async Task AddHighProtectionAlertIfNeeded(
        Guid organizationId,
        string targetType,
        Guid targetId,
        string targetName,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var recentStart = now.AddHours(-1);
        var recentBlocked = await _context.LandingPageSubmissionAttempts
            .IgnoreQueryFilters()
            .CountAsync(attempt =>
                !attempt.IsDeleted &&
                attempt.OrganizationId == organizationId &&
                attempt.TargetType == targetType &&
                attempt.TargetId == targetId &&
                attempt.Blocked &&
                attempt.AttemptedAtUtc >= recentStart,
                cancellationToken);
        if (recentBlocked + 1 < 20)
        {
            return;
        }

        var alreadyAlerted = await _context.LandingPageAuditEntries
            .IgnoreQueryFilters()
            .AnyAsync(entry =>
                !entry.IsDeleted &&
                entry.OrganizationId == organizationId &&
                entry.EntityType == targetType &&
                entry.EntityId == targetId &&
                entry.Action == "HighProtectionAlert" &&
                entry.OccurredAtUtc >= recentStart,
                cancellationToken);
        if (alreadyAlerted)
        {
            return;
        }

        _context.LandingPageAuditEntries.Add(LandingPageAudit.Create(
            organizationId,
            targetType,
            targetId,
            "HighProtectionAlert",
            "Alerta alto de protecao",
            $"{targetName} recebeu {recentBlocked + 1} bloqueios na ultima hora.",
            now,
            null));

        var hasOpenOperationalAlert = await _context.OperationalAlerts
            .IgnoreQueryFilters()
            .AnyAsync(alert =>
                !alert.IsDeleted &&
                alert.OrganizationId == organizationId &&
                alert.Source == "LandingProtection" &&
                alert.RelatedEntityType == targetType &&
                alert.RelatedEntityId == targetId &&
                alert.Status != OperationalAlertStatus.Resolved,
                cancellationToken);
        if (hasOpenOperationalAlert)
        {
            return;
        }

        _context.OperationalAlerts.Add(OperationalAlert.Create(
            organizationId,
            "Protecao da landing em alerta alto",
            $"{targetName} recebeu {recentBlocked + 1} bloqueios na ultima hora. Revise as regras, origem e fingerprints.",
            OperationalAlertSeverity.High,
            "LandingProtection",
            targetType,
            targetId,
            $"/captacao/protecao?targetType={targetType}&targetId={targetId}&blocked=true",
            null,
            now.AddHours(4),
            now));
    }

    private static LandingPageSubmissionAttempt SubmissionAttempt(
        Guid organizationId,
        string targetType,
        Guid targetId,
        string fingerprintHash,
        string source,
        bool blocked,
        string? reason,
        DateTimeOffset attemptedAtUtc)
    {
        return new LandingPageSubmissionAttempt
        {
            OrganizationId = organizationId,
            TargetType = targetType,
            TargetId = targetId,
            FingerprintHash = fingerprintHash,
            Source = source,
            Blocked = blocked,
            Reason = reason,
            AttemptedAtUtc = attemptedAtUtc,
        };
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

    private static string LandingTimelineDescription(
        string targetName,
        decimal? amount,
        string sourceDescription,
        IReadOnlyDictionary<string, string> customFields)
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

        foreach (var field in customFields.Where(field => !string.IsNullOrWhiteSpace(field.Value)))
        {
            parts.Add($"{field.Key}: {field.Value.Trim()}");
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
