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
                $"Lead gerado pela landing page de {targetType}.");
            donor.ReplaceEmails(email is null ? [] : [DonorEmail.Create(targetOrganizationId, donor.Id, EmailType.Personal, email, true)]);
            donor.ReplacePhones(phone is null ? [] : [DonorPhone.Create(targetOrganizationId, donor.Id, PhoneType.WhatsApp, phone, true)]);
            _context.Donors.Add(donor);
            created = true;
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = targetOrganizationId,
            DonorId = donor.Id,
            Type = TimelineEntryType.Note,
            Title = "Interesse pela landing page",
            Description = targetName,
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            RelatedEntityType = targetType,
            RelatedEntityId = request.TargetId,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new PublicLeadSubmissionDto { DonorId = donor.Id, Created = created };
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
}
