using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Donors.Commands.CreateDonor;

public record CreateDonorCommand : IRequest<Guid>
{
    public string FullName { get; init; } = string.Empty;
    public Guid PersonTypeOptionId { get; init; }
    public Guid StatusOptionId { get; init; }
    public Guid? SourceOptionId { get; init; }
    public Guid? RelationshipProfileOptionId { get; init; }
    public Guid? PreferredContactChannelOptionId { get; init; }
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
    public string? AssignedUserId { get; init; }
    public Guid? AcquisitionCampaignId { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyCollection<Guid> TagIds { get; init; } = [];
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
            PersonTypeOptionId = request.PersonTypeOptionId,
            StatusOptionId = request.StatusOptionId,
            SourceOptionId = request.SourceOptionId,
            RelationshipProfileOptionId = request.RelationshipProfileOptionId,
            PreferredContactChannelOptionId = request.PreferredContactChannelOptionId,
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
            AssignedUserId = request.AssignedUserId,
            AcquisitionCampaignId = request.AcquisitionCampaignId,
            Notes = request.Notes?.Trim(),
        };

        foreach (var tagId in request.TagIds.Distinct())
        {
            donor.TagAssignments.Add(new DonorTagAssignment
            {
                OrganizationId = organizationId,
                DonorTagId = tagId,
            });
        }

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync(cancellationToken);

        return donor.Id;
    }
}
