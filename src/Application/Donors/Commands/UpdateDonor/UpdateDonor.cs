using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donors.Commands.UpdateDonor;

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
    public string? Notes { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; } = [];
}

public sealed class UpdateDonorCommandHandler : IRequestHandler<UpdateDonorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateDonorCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateDonorCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var donor = await _context.Donors
            .Include(entity => entity.TagAssignments)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (donor is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donor), request.Id.ToString());
        }

        donor.FullName = request.FullName.Trim();
        donor.PersonTypeOptionId = await OptionLookup.RequiredIdAsync(_context, "DonorPersonType", request.PersonType, cancellationToken);
        donor.StatusOptionId = await OptionLookup.RequiredIdAsync(_context, "DonorStatus", request.DoNotContact ? "DoNotContact" : request.Status, cancellationToken);
        donor.SourceOptionId = string.IsNullOrWhiteSpace(request.Source)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, "DonorSource", request.Source, cancellationToken);
        donor.RelationshipProfileOptionId = string.IsNullOrWhiteSpace(request.RelationshipProfile)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, "RelationshipProfile", request.RelationshipProfile, cancellationToken);
        donor.PreferredContactChannelOptionId = string.IsNullOrWhiteSpace(request.PreferredContactChannel)
            ? null
            : await OptionLookup.RequiredIdAsync(_context, "ContactChannel", request.PreferredContactChannel, cancellationToken);
        donor.Document = request.Document?.Trim();
        donor.Email = request.Email?.Trim();
        donor.Phone = request.Phone?.Trim();
        donor.WhatsApp = request.WhatsApp?.Trim();
        donor.City = request.City?.Trim();
        donor.State = request.State?.Trim();
        donor.AllowsCommunication = request.AllowsCommunication;
        donor.DoNotContact = request.DoNotContact;
        donor.DoNotContactReason = request.DoNotContactReason?.Trim();
        donor.AssignedUserId = request.AssignedUserId;
        donor.Notes = request.Notes?.Trim();

        donor.TagAssignments.Clear();
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

        await _context.SaveChangesAsync(cancellationToken);
    }
}
