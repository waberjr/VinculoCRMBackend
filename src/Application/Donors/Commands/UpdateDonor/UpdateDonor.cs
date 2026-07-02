using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;
using FluentValidation.Results;

namespace VinculoBackend.Application.Donors.Commands.UpdateDonor;

public sealed record DonorPhoneRequest(string TypeCode, string Number, bool IsPrimary);

public sealed record DonorEmailRequest(string TypeCode, string Address, bool IsPrimary);

public record UpdateDonorCommand : IRequest
{
    public Guid Id { get; init; }
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
    public string? City { get; init; }
    public string? State { get; init; }
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

public sealed class UpdateDonorCommandHandler : IRequestHandler<UpdateDonorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IBrazilianDocumentValidator _documentValidator;
    private readonly IOrganizationContext _organizationContext;

    public UpdateDonorCommandHandler(
        IApplicationDbContext context,
        IBrazilianDocumentValidator documentValidator,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _documentValidator = documentValidator;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateDonorCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donor = await _context.Donors
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (donor is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.Id.ToString());
        }

        var normalizedDocument = NormalizeDocument(request.Document);
        await EnsureDocumentIsUniqueAsync(normalizedDocument, donor.Id, cancellationToken);
        var previousAllowsCommunication = donor.AllowsCommunication;
        var previousDoNotContact = donor.DoNotContact;

        donor.FullName = request.FullName.Trim();
        donor.PersonType = SystemOptionMapper.Parse<DonorPersonType>(request.PersonType);
        donor.Status = request.DoNotContact ? DonorStatus.DoNotContact : SystemOptionMapper.Parse<DonorStatus>(request.Status);
        donor.SourceOptionId = string.IsNullOrWhiteSpace(request.Source)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.DonorSource, request.Source, cancellationToken);
        donor.RelationshipProfileOptionId = string.IsNullOrWhiteSpace(request.RelationshipProfile)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.RelationshipProfile, request.RelationshipProfile, cancellationToken);
        donor.PreferredContactChannelOptionId = string.IsNullOrWhiteSpace(request.PreferredContactChannel)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, ConfigurableOptionCategory.ContactChannel, request.PreferredContactChannel, cancellationToken);
        donor.Document = normalizedDocument;
        donor.Email = request.Email?.Trim();
        donor.Phone = request.Phone?.Trim();
        donor.WhatsApp = request.WhatsApp?.Trim();
        donor.City = request.City?.Trim();
        donor.State = request.State?.Trim();
        donor.AllowsCommunication = request.AllowsCommunication;
        donor.DoNotContact = request.DoNotContact;
        donor.DoNotContactReason = request.DoNotContactReason?.Trim();
        donor.AssignedUserId = request.AssignedUserId;
        donor.AcquisitionCampaignId = request.AcquisitionCampaignId;
        donor.Notes = request.Notes?.Trim();

        await ApplyContactsAsync(donor, request, organizationId, cancellationToken);

        var currentAssignments = await _context.DonorTagAssignments
            .Where(assignment => assignment.DonorId == donor.Id)
            .ToListAsync(cancellationToken);

        _context.DonorTagAssignments.RemoveRange(currentAssignments);

        foreach (var tagName in request.Tags.Select(tag => tag.Trim()).Where(tag => tag.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedTag = tagName.ToLower();
            var tagId = await _context.DonorTags
                .Where(tag => tag.Name.ToLower() == normalizedTag)
                .Select(tag => (Guid?)tag.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (tagId is null)
            {
                var donorTag = new DonorTag
                {
                    OrganizationId = organizationId,
                    Name = tagName,
                    IsActive = true,
                };
                _context.DonorTags.Add(donorTag);
                tagId = donorTag.Id;
            }

            donor.TagAssignments.Add(new DonorTagAssignment
            {
                OrganizationId = organizationId,
                DonorTagId = tagId.Value,
            });
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donor.Id,
            Type = TimelineEntryType.Note,
            Title = "Doador atualizado",
            Description = donor.FullName,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donor),
            RelatedEntityId = donor.Id,
        });

        if (previousAllowsCommunication != donor.AllowsCommunication || previousDoNotContact != donor.DoNotContact)
        {
            _context.DonorTimelineEntries.Add(new DonorTimelineEntry
            {
                OrganizationId = organizationId,
                DonorId = donor.Id,
                Type = TimelineEntryType.Contact,
                Title = "Consentimento de comunicacao atualizado",
                Description = donor.DoNotContact ? donor.DoNotContactReason : null,
                OccurredAtUtc = DateTimeOffset.UtcNow,
                RelatedEntityType = nameof(Donor),
                RelatedEntityId = donor.Id,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private string? NormalizeDocument(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return null;
        }

        return _documentValidator.Normalize(document);
    }

    private async Task EnsureDocumentIsUniqueAsync(string? document, Guid donorId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return;
        }

        var alreadyExists = await _context.Donors
            .AsNoTracking()
            .AnyAsync(donor => donor.Document == document && donor.Id != donorId, cancellationToken);

        if (alreadyExists)
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(UpdateDonorCommand.Document), "CPF/CNPJ ja cadastrado nesta organizacao."),
            ]);
        }
    }

    private async Task ApplyContactsAsync(Donor donor, UpdateDonorCommand request, Guid organizationId, CancellationToken cancellationToken)
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

        var currentPhones = await _context.DonorPhones
            .Where(phone => phone.DonorId == donor.Id)
            .ToListAsync(cancellationToken);
        var currentEmails = await _context.DonorEmails
            .Where(email => email.DonorId == donor.Id)
            .ToListAsync(cancellationToken);

        _context.DonorPhones.RemoveRange(currentPhones);
        _context.DonorEmails.RemoveRange(currentEmails);

        var phoneIndex = 0;
        foreach (var phone in phones.Where(phone => !string.IsNullOrWhiteSpace(phone.Number)))
        {
            var type = string.IsNullOrWhiteSpace(phone.TypeCode) ? PhoneType.Mobile : SystemOptionMapper.Parse<PhoneType>(phone.TypeCode);
            donor.Phones.Add(new DonorPhone
            {
                OrganizationId = organizationId,
                DonorId = donor.Id,
                Type = type,
                Number = phone.Number.Trim(),
                IsPrimary = phone.IsPrimary || phoneIndex == 0,
            });
            phoneIndex++;
        }

        var emailIndex = 0;
        foreach (var email in emails.Where(email => !string.IsNullOrWhiteSpace(email.Address)))
        {
            var type = string.IsNullOrWhiteSpace(email.TypeCode) ? EmailType.Personal : SystemOptionMapper.Parse<EmailType>(email.TypeCode);
            donor.Emails.Add(new DonorEmail
            {
                OrganizationId = organizationId,
                DonorId = donor.Id,
                Type = type,
                Address = email.Address.Trim(),
                IsPrimary = email.IsPrimary || emailIndex == 0,
            });
            emailIndex++;
        }

        donor.Phone = donor.Phones.OrderByDescending(phone => phone.IsPrimary).Select(phone => phone.Number).FirstOrDefault();
        donor.WhatsApp = phones.FirstOrDefault(phone => !string.IsNullOrWhiteSpace(phone.TypeCode) && SystemOptionMapper.Parse<PhoneType>(phone.TypeCode) == PhoneType.WhatsApp)?.Number?.Trim() ?? donor.Phone;
        donor.Email = donor.Emails.OrderByDescending(email => email.IsPrimary).Select(email => email.Address).FirstOrDefault();
    }
}
