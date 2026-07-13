using VinculoBackend.Domain.Enums;
using VinculoBackend.Domain.Exceptions;

namespace VinculoBackend.Domain.Entities;

public class Donor : OrganizationEntity
{
    public string FullName { get; set; } = string.Empty;
    public DonorPersonType PersonType { get; set; }
    public string? Document { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public DonorStatus Status { get; set; }
    public Guid? SourceOptionId { get; set; }
    public ConfigurableOption? SourceOption { get; set; }
    public Guid? RelationshipProfileOptionId { get; set; }
    public ConfigurableOption? RelationshipProfileOption { get; set; }
    public Guid? PreferredContactChannelOptionId { get; set; }
    public ConfigurableOption? PreferredContactChannelOption { get; set; }
    public bool AllowsCommunication { get; set; } = true;
    public bool DoNotContact { get; set; }
    public string? DoNotContactReason { get; set; }
    public string? AssignedUserId { get; set; }
    public Guid? AcquisitionCampaignId { get; set; }
    public Campaign? AcquisitionCampaign { get; set; }
    public string? Notes { get; set; }
    public ICollection<DonorPhone> Phones { get; } = new List<DonorPhone>();
    public ICollection<DonorEmail> Emails { get; } = new List<DonorEmail>();
    public ICollection<DonorTagAssignment> TagAssignments { get; } = new List<DonorTagAssignment>();

    public static Donor Create(
        Guid organizationId,
        string fullName,
        DonorPersonType personType,
        DonorStatus status,
        Guid? sourceOptionId,
        Guid? relationshipProfileOptionId,
        Guid? preferredContactChannelOptionId,
        string? document,
        string? email,
        string? phone,
        string? whatsapp,
        DateOnly? birthDate,
        string? city,
        string? state,
        string? addressLine1,
        string? addressLine2,
        string? postalCode,
        bool allowsCommunication,
        bool doNotContact,
        string? doNotContactReason,
        string? assignedUserId,
        Guid? acquisitionCampaignId,
        string? notes)
    {
        var donor = new Donor { OrganizationId = organizationId };
        donor.Update(
            fullName,
            personType,
            status,
            sourceOptionId,
            relationshipProfileOptionId,
            preferredContactChannelOptionId,
            document,
            email,
            phone,
            whatsapp,
            birthDate,
            city,
            state,
            addressLine1,
            addressLine2,
            postalCode,
            allowsCommunication,
            doNotContact,
            doNotContactReason,
            assignedUserId,
            acquisitionCampaignId,
            notes);

        return donor;
    }

    public void Update(
        string fullName,
        DonorPersonType personType,
        DonorStatus status,
        Guid? sourceOptionId,
        Guid? relationshipProfileOptionId,
        Guid? preferredContactChannelOptionId,
        string? document,
        string? email,
        string? phone,
        string? whatsapp,
        DateOnly? birthDate,
        string? city,
        string? state,
        string? addressLine1,
        string? addressLine2,
        string? postalCode,
        bool allowsCommunication,
        bool doNotContact,
        string? doNotContactReason,
        string? assignedUserId,
        Guid? acquisitionCampaignId,
        string? notes)
    {
        SetFullName(fullName);
        PersonType = personType;
        SourceOptionId = sourceOptionId;
        RelationshipProfileOptionId = relationshipProfileOptionId;
        PreferredContactChannelOptionId = preferredContactChannelOptionId;
        Document = TrimToNull(document);
        Email = TrimToNull(email);
        Phone = TrimToNull(phone);
        WhatsApp = TrimToNull(whatsapp);
        BirthDate = birthDate;
        City = TrimToNull(city);
        State = TrimToNull(state);
        AddressLine1 = TrimToNull(addressLine1);
        AddressLine2 = TrimToNull(addressLine2);
        PostalCode = TrimToNull(postalCode);
        AssignedUserId = assignedUserId;
        AcquisitionCampaignId = acquisitionCampaignId;
        Notes = TrimToNull(notes);
        ApplyCommunicationConsent(status, allowsCommunication, doNotContact, doNotContactReason);
    }

    public void BlockContact(string? reason)
    {
        ApplyCommunicationConsent(DonorStatus.DoNotContact, false, true, reason);
    }

    public void ReplacePhones(IEnumerable<DonorPhone> phones)
    {
        var validPhones = PreparePhonesForReplacement(phones);

        Phones.Clear();
        foreach (var phone in validPhones)
        {
            Phones.Add(phone);
        }
    }

    public void ReplaceEmails(IEnumerable<DonorEmail> emails)
    {
        var validEmails = PrepareEmailsForReplacement(emails);

        Emails.Clear();
        foreach (var email in validEmails)
        {
            Emails.Add(email);
        }
    }

    public IReadOnlyCollection<DonorPhone> PreparePhonesForReplacement(IEnumerable<DonorPhone> phones)
    {
        var validPhones = phones
            .Where(phone => !string.IsNullOrWhiteSpace(phone.Number))
            .ToList();

        NormalizePrimary(validPhones, phone => phone.IsPrimary, (phone, isPrimary) => phone.SetPrimary(isPrimary));

        Phone = validPhones.OrderByDescending(phone => phone.IsPrimary).Select(phone => phone.Number).FirstOrDefault();
        WhatsApp = validPhones
            .Where(phone => phone.Type == PhoneType.WhatsApp)
            .OrderByDescending(phone => phone.IsPrimary)
            .Select(phone => phone.Number)
            .FirstOrDefault() ?? Phone;

        return validPhones;
    }

    public IReadOnlyCollection<DonorEmail> PrepareEmailsForReplacement(IEnumerable<DonorEmail> emails)
    {
        var validEmails = emails
            .Where(email => !string.IsNullOrWhiteSpace(email.Address))
            .ToList();

        NormalizePrimary(validEmails, email => email.IsPrimary, (email, isPrimary) => email.SetPrimary(isPrimary));

        Email = validEmails.OrderByDescending(email => email.IsPrimary).Select(email => email.Address).FirstOrDefault();

        return validEmails;
    }

    private void ApplyCommunicationConsent(
        DonorStatus requestedStatus,
        bool allowsCommunication,
        bool doNotContact,
        string? doNotContactReason)
    {
        if (doNotContact)
        {
            var reason = TrimToNull(doNotContactReason);
            if (reason is null)
            {
                throw new DomainValidationException("Informe o motivo para nao contatar o doador.");
            }

            Status = DonorStatus.DoNotContact;
            AllowsCommunication = false;
            DoNotContact = true;
            DoNotContactReason = reason;
            return;
        }

        if (requestedStatus == DonorStatus.DoNotContact)
        {
            throw new DomainValidationException("Use o bloqueio de contato com motivo para marcar o doador como nao contatar.");
        }

        Status = requestedStatus;
        AllowsCommunication = allowsCommunication;
        DoNotContact = false;
        DoNotContactReason = null;
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException("Informe o nome do doador.");
        }

        FullName = fullName.Trim();
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static void NormalizePrimary<T>(
        IReadOnlyList<T> items,
        Func<T, bool> isPrimary,
        Action<T, bool> setPrimary)
    {
        if (items.Count == 0)
        {
            return;
        }

        var primaryIndex = 0;
        for (var index = 0; index < items.Count; index++)
        {
            if (isPrimary(items[index]))
            {
                primaryIndex = index;
                break;
            }
        }

        for (var index = 0; index < items.Count; index++)
        {
            setPrimary(items[index], index == primaryIndex);
        }
    }
}
