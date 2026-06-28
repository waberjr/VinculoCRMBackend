using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Donors.Models;

public sealed class DonorTagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class DonorListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Document { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public bool AllowsCommunication { get; init; }
    public bool DoNotContact { get; init; }
    public OptionDto Status { get; init; } = new();
    public OptionDto? RelationshipProfile { get; init; }
    public IReadOnlyCollection<DonorTagDto> Tags { get; init; } = [];
    public decimal TotalDonated { get; init; }
    public DateTimeOffset? LastDonationAtUtc { get; init; }
    public DateTimeOffset Created { get; init; }
}

public sealed class DonorDetailDto : DonorListItemDto
{
    public string? WhatsApp { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? PostalCode { get; init; }
    public string? Notes { get; init; }
    public string? AssignedUserId { get; init; }
    public OptionDto PersonType { get; init; } = new();
    public OptionDto? Source { get; init; }
    public OptionDto? PreferredContactChannel { get; init; }
}
