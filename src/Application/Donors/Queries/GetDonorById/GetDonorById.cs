using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;

namespace VinculoBackend.Application.Donors.Queries.GetDonorById;

public record GetDonorByIdQuery(Guid Id) : IRequest<DonorDetailDto?>;

public sealed class GetDonorByIdQueryHandler : IRequestHandler<GetDonorByIdQuery, DonorDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonorByIdQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<DonorDetailDto?> Handle(GetDonorByIdQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        return await _context.Donors
            .AsNoTracking()
            .Where(donor => donor.Id == request.Id)
            .Select(donor => new DonorDetailDto
            {
                Id = donor.Id,
                FullName = donor.FullName,
                Document = donor.Document,
                Email = donor.Email,
                Phone = donor.Phone,
                WhatsApp = donor.WhatsApp,
                BirthDate = donor.BirthDate,
                City = donor.City,
                State = donor.State,
                AddressLine1 = donor.AddressLine1,
                AddressLine2 = donor.AddressLine2,
                PostalCode = donor.PostalCode,
                Notes = donor.Notes,
                AssignedUserId = donor.AssignedUserId,
                AllowsCommunication = donor.AllowsCommunication,
                DoNotContact = donor.DoNotContact,
                Created = donor.Created,
                Status = new OptionDto { Id = donor.StatusOption.Id, Category = donor.StatusOption.Category, Code = donor.StatusOption.Code, Name = donor.StatusOption.Name, Color = donor.StatusOption.Color, SortOrder = donor.StatusOption.SortOrder, IsSystem = donor.StatusOption.IsSystem, IsActive = donor.StatusOption.IsActive },
                PersonType = new OptionDto { Id = donor.PersonTypeOption.Id, Category = donor.PersonTypeOption.Category, Code = donor.PersonTypeOption.Code, Name = donor.PersonTypeOption.Name, Color = donor.PersonTypeOption.Color, SortOrder = donor.PersonTypeOption.SortOrder, IsSystem = donor.PersonTypeOption.IsSystem, IsActive = donor.PersonTypeOption.IsActive },
                Source = donor.SourceOption == null ? null : new OptionDto { Id = donor.SourceOption.Id, Category = donor.SourceOption.Category, Code = donor.SourceOption.Code, Name = donor.SourceOption.Name, Color = donor.SourceOption.Color, SortOrder = donor.SourceOption.SortOrder, IsSystem = donor.SourceOption.IsSystem, IsActive = donor.SourceOption.IsActive },
                RelationshipProfile = donor.RelationshipProfileOption == null ? null : new OptionDto { Id = donor.RelationshipProfileOption.Id, Category = donor.RelationshipProfileOption.Category, Code = donor.RelationshipProfileOption.Code, Name = donor.RelationshipProfileOption.Name, Color = donor.RelationshipProfileOption.Color, SortOrder = donor.RelationshipProfileOption.SortOrder, IsSystem = donor.RelationshipProfileOption.IsSystem, IsActive = donor.RelationshipProfileOption.IsActive },
                PreferredContactChannel = donor.PreferredContactChannelOption == null ? null : new OptionDto { Id = donor.PreferredContactChannelOption.Id, Category = donor.PreferredContactChannelOption.Category, Code = donor.PreferredContactChannelOption.Code, Name = donor.PreferredContactChannelOption.Name, Color = donor.PreferredContactChannelOption.Color, SortOrder = donor.PreferredContactChannelOption.SortOrder, IsSystem = donor.PreferredContactChannelOption.IsSystem, IsActive = donor.PreferredContactChannelOption.IsActive },
                Tags = donor.TagAssignments.OrderBy(tag => tag.DonorTag.Name).Select(tag => new DonorTagDto { Id = tag.DonorTagId, Name = tag.DonorTag.Name }).ToList(),
                TotalDonated = _context.Donations.Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null).Sum(donation => (decimal?)donation.Amount) ?? 0,
                LastDonationAtUtc = _context.Donations.Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null).Max(donation => (DateTimeOffset?)donation.PaidAtUtc),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
