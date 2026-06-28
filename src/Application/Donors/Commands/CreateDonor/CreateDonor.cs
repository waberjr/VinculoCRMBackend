using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donors.Commands.CreateDonor;

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
    public IReadOnlyCollection<string> Tags { get; init; } = [];
}

public sealed class CreateDonorCommandHandler : IRequestHandler<CreateDonorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CreateDonorCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

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
            Document = request.Document?.Trim(),
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
        await _context.SaveChangesAsync(cancellationToken);

        return donor.Id;
    }
}
