using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
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

    public CreateDonorCommandHandler(
        IApplicationDbContext context,
        IBrazilianDocumentValidator documentValidator,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _documentValidator = documentValidator;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var normalizedDocument = NormalizeDocument(request.Document);
        await EnsureDocumentIsUniqueAsync(normalizedDocument, null, cancellationToken);

        var donor = new Donor
        {
            OrganizationId = organizationId,
            FullName = request.FullName.Trim(),
            PersonTypeOptionId = await OptionLookup.RequiredIdAsync(_context, "DonorPersonType", request.PersonType, cancellationToken),
            StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonorStatus", request.DoNotContact ? "DoNotContact" : request.Status, cancellationToken),
            SourceOptionId = string.IsNullOrWhiteSpace(request.Source)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, "DonorSource", request.Source, cancellationToken),
            RelationshipProfileOptionId = string.IsNullOrWhiteSpace(request.RelationshipProfile)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, "RelationshipProfile", request.RelationshipProfile, cancellationToken),
            PreferredContactChannelOptionId = string.IsNullOrWhiteSpace(request.PreferredContactChannel)
                ? null
                : await OptionLookup.RequiredIdAsync(_context, "ContactChannel", request.PreferredContactChannel, cancellationToken),
            Document = normalizedDocument,
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            WhatsApp = request.WhatsApp?.Trim(),
            BirthDate = request.BirthDate,
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            AddressLine1 = request.AddressLine1?.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            AllowsCommunication = request.AllowsCommunication,
            DoNotContact = request.DoNotContact,
            DoNotContactReason = request.DoNotContactReason?.Trim(),
            AssignedUserId = request.AssignedUserId,
            AcquisitionCampaignId = request.AcquisitionCampaignId,
            Notes = request.Notes?.Trim(),
        };

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

        _context.Donors.Add(donor);
        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donor.Id,
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", "Note", cancellationToken),
            Title = "Doador criado",
            Description = donor.FullName,
            OccurredAtUtc = DateTimeOffset.UtcNow,
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
                new ValidationFailure(nameof(CreateDonorCommand.Document), "CPF/CNPJ ja cadastrado nesta organizacao."),
            ]);
        }
    }

    private async Task ApplyContactsAsync(Donor donor, CreateDonorCommand request, Guid organizationId, CancellationToken cancellationToken)
    {
        var phones = request.Phones.Count > 0
            ? request.Phones
            : string.IsNullOrWhiteSpace(request.Phone)
                ? []
                : [new DonorPhoneRequest(string.IsNullOrWhiteSpace(request.WhatsApp) ? "Mobile" : "WhatsApp", request.Phone, true)];
        var emails = request.Emails.Count > 0
            ? request.Emails
            : string.IsNullOrWhiteSpace(request.Email)
                ? []
                : [new DonorEmailRequest("Personal", request.Email, true)];

        var phoneIndex = 0;
        foreach (var phone in phones.Where(phone => !string.IsNullOrWhiteSpace(phone.Number)))
        {
            var typeCode = string.IsNullOrWhiteSpace(phone.TypeCode) ? "Mobile" : phone.TypeCode;
            donor.Phones.Add(new DonorPhone
            {
                OrganizationId = organizationId,
                TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "PhoneType", typeCode, cancellationToken),
                Number = phone.Number.Trim(),
                IsPrimary = phone.IsPrimary || phoneIndex == 0,
            });
            phoneIndex++;
        }

        var emailIndex = 0;
        foreach (var email in emails.Where(email => !string.IsNullOrWhiteSpace(email.Address)))
        {
            var typeCode = string.IsNullOrWhiteSpace(email.TypeCode) ? "Personal" : email.TypeCode;
            donor.Emails.Add(new DonorEmail
            {
                OrganizationId = organizationId,
                TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "EmailType", typeCode, cancellationToken),
                Address = email.Address.Trim(),
                IsPrimary = email.IsPrimary || emailIndex == 0,
            });
            emailIndex++;
        }

        donor.Phone = donor.Phones.OrderByDescending(phone => phone.IsPrimary).Select(phone => phone.Number).FirstOrDefault();
        donor.WhatsApp = phones.FirstOrDefault(phone => ConfigurableOptionCode.FromName(phone.TypeCode) == "whats-app")?.Number?.Trim() ?? donor.Phone;
        donor.Email = donor.Emails.OrderByDescending(email => email.IsPrimary).Select(email => email.Address).FirstOrDefault();
    }
}
