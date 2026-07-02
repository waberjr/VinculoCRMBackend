using VinculoBackend.Domain.Enums;

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
}
