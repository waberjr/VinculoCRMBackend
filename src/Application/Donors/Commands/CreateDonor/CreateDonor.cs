using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Donors.Commands.CreateDonor;

public sealed record DonorPhoneRequest(string TypeCode, string Number, bool IsPrimary);

public sealed record DonorEmailRequest(string TypeCode, string Address, bool IsPrimary);

public record CreateDonorCommand : IRequest<Guid>
{
    public string FullName { get; init; } = string.Empty;
    public string PersonType { get; init; } = "Individual";
    public string Status { get; init; } = "Lead";
    public string? Source { get; init; }
    public string? RelationshipProfile { get; init; }
    public string? PreferredContactChannel { get; init; }
    public string? Document { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? WhatsApp { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? PostalCode { get; init; }
    public bool AllowsCommunication { get; init; } = true;
    public bool DoNotContact { get; init; }
    public string? DoNotContactReason { get; init; }
    public string? AssignedUserId { get; init; }
    public Guid? AcquisitionCampaignId { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyCollection<DonorPhoneRequest> Phones { get; init; } = [];
    public IReadOnlyCollection<DonorEmailRequest> Emails { get; init; } = [];
    public IReadOnlyCollection<string> Tags { get; init; } = [];
}

public sealed class CreateDonorCommandHandler : IRequestHandler<CreateDonorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IBrazilianDocumentValidator _documentValidator;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public CreateDonorCommandHandler(
        IApplicationDbContext context,
        IBrazilianDocumentValidator documentValidator,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _documentValidator = documentValidator;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var normalizedDocument = NormalizeDocument(request.Document);
        await EnsureDocumentIsUniqueAsync(normalizedDocument, null, cancellationToken);

        var donor = Donor.Create(
            organizationId,
            request.FullName,
            SystemOptionMapper.Parse<DonorPersonType>(request.PersonType),
            SystemOptionMapper.Parse<DonorStatus>(request.Status),
            string.IsNullOrWhiteSpace(request.Source)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.DonorSource, request.Source, cancellationToken),
            string.IsNullOrWhiteSpace(request.RelationshipProfile)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.RelationshipProfile, request.RelationshipProfile, cancellationToken),
            string.IsNullOrWhiteSpace(request.PreferredContactChannel)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.ContactChannel, request.PreferredContactChannel, cancellationToken),
            normalizedDocument,
            request.Email,
            request.Phone,
            request.WhatsApp,
            request.BirthDate,
            request.City,
            request.State,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.AllowsCommunication,
            request.DoNotContact,
            request.DoNotContactReason,
            request.AssignedUserId,
            request.AcquisitionCampaignId,
            request.Notes);

        await ApplyContactsAsync(donor, request, organizationId, cancellationToken);

        foreach (var tagName in request.Tags.Select(tag => tag.Trim()).Where(tag => tag.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedTag = tagName.ToLower();
            var tagId = await _context.DonorTags
                .Where(tag => tag.Name.ToLower() == normalizedTag)
                .Select(tag => (Guid?)tag.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (tagId is null)
            {
                var donorTag = DonorTag.Create(organizationId, tagName);
                _context.DonorTags.Add(donorTag);
                tagId = donorTag.Id;
            }

            donor.TagAssignments.Add(new DonorTagAssignment
            {
                OrganizationId = organizationId,
                DonorTagId = tagId.Value,
            });
        }

        _context.Donors.Add(donor);
        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donor.Id,
            Type = TimelineEntryType.Note,
            Title = "Doador criado",
            Description = donor.FullName,
            OccurredAtUtc = _timeProvider.GetUtcNow(),
            RelatedEntityType = nameof(Donor),
            RelatedEntityId = donor.Id,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return donor.Id;
    }

    private string? NormalizeDocument(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return null;
        }

        return _documentValidator.Normalize(document);
    }

    private async Task EnsureDocumentIsUniqueAsync(string? document, Guid? donorId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return;
        }

        var alreadyExists = await _context.Donors
            .AsNoTracking()
            .AnyAsync(donor => donor.Document == document && (donorId == null || donor.Id != donorId), cancellationToken);

        if (alreadyExists)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(CreateDonorCommand.Document), "CPF/CNPJ já cadastrado nestá organização."),
            ]);
        }
    }

    private async Task ApplyContactsAsync(Donor donor, CreateDonorCommand request, Guid organizationId, CancellationToken cancellationToken)
    {
        var phones = request.Phones.Count > 0
            ? request.Phones
            : string.IsNullOrWhiteSpace(request.Phone)
                ? []
                : [new DonorPhoneRequest(string.IsNullOrWhiteSpace(request.WhatsApp) ? PhoneType.Mobile.ToString() : PhoneType.WhatsApp.ToString(), request.Phone, true)];
        var emails = request.Emails.Count > 0
            ? request.Emails
            : string.IsNullOrWhiteSpace(request.Email)
                ? []
                : [new DonorEmailRequest(EmailType.Personal.ToString(), request.Email, true)];

        donor.ReplacePhones(phones
            .Where(phone => !string.IsNullOrWhiteSpace(phone.Number))
            .Select(phone => DonorPhone.Create(
                organizationId,
                donor.Id,
                string.IsNullOrWhiteSpace(phone.TypeCode) ? PhoneType.Mobile : SystemOptionMapper.Parse<PhoneType>(phone.TypeCode),
                phone.Number,
                phone.IsPrimary)));

        donor.ReplaceEmails(emails
            .Where(email => !string.IsNullOrWhiteSpace(email.Address))
            .Select(email => DonorEmail.Create(
                organizationId,
                donor.Id,
                string.IsNullOrWhiteSpace(email.TypeCode) ? EmailType.Personal : SystemOptionMapper.Parse<EmailType>(email.TypeCode),
                email.Address,
                email.IsPrimary)));
    }
}
