using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;

namespace VinculoBackend.Application.Donors.Queries.GetDonors;

public record GetDonorsQuery : IRequest<PaginatedResult<DonorListItemDto>>
{
    public string? Search { get; init; }
    public Guid? StatusOptionId { get; init; }
    public Guid? TagId { get; init; }
    public Guid? RelationshipProfileOptionId { get; init; }
    public bool? AllowsCommunication { get; init; }
    public bool? DoNotContact { get; init; }
    public string? State { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetDonorsQueryHandler : IRequestHandler<GetDonorsQuery, PaginatedResult<DonorListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetDonorsQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<PaginatedResult<DonorListItemDto>> Handle(GetDonorsQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var query = _context.Donors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(donor =>
                donor.FullName.ToLower().Contains(search) ||
                (donor.Email != null && donor.Email.ToLower().Contains(search)) ||
                (donor.Document != null && donor.Document.Contains(search)) ||
                (donor.Phone != null && donor.Phone.Contains(search)));
        }

        if (request.StatusOptionId is not null)
        {
            query = query.Where(donor => donor.StatusOptionId == request.StatusOptionId);
        }

        if (request.RelationshipProfileOptionId is not null)
        {
            query = query.Where(donor => donor.RelationshipProfileOptionId == request.RelationshipProfileOptionId);
        }

        if (request.TagId is not null)
        {
            query = query.Where(donor => donor.TagAssignments.Any(tag => tag.DonorTagId == request.TagId));
        }

        if (request.AllowsCommunication is not null)
        {
            query = query.Where(donor => donor.AllowsCommunication == request.AllowsCommunication);
        }

        if (request.DoNotContact is not null)
        {
            query = query.Where(donor => donor.DoNotContact == request.DoNotContact);
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            query = query.Where(donor => donor.State == request.State);
        }

        var projected = query
            .OrderBy(donor => donor.FullName)
            .Select(donor => new DonorListItemDto
            {
                Id = donor.Id,
                FullName = donor.FullName,
                Document = donor.Document,
                Email = donor.Email,
                Phone = donor.Phone,
                City = donor.City,
                State = donor.State,
                AllowsCommunication = donor.AllowsCommunication,
                DoNotContact = donor.DoNotContact,
                Created = donor.Created,
                Status = new OptionDto
                {
                    Id = donor.StatusOption.Id,
                    Category = donor.StatusOption.Category,
                    Code = donor.StatusOption.Code,
                    Name = donor.StatusOption.Name,
                    Color = donor.StatusOption.Color,
                    SortOrder = donor.StatusOption.SortOrder,
                    IsSystem = donor.StatusOption.IsSystem,
                    IsActive = donor.StatusOption.IsActive,
                },
                RelationshipProfile = donor.RelationshipProfileOption == null ? null : new OptionDto
                {
                    Id = donor.RelationshipProfileOption.Id,
                    Category = donor.RelationshipProfileOption.Category,
                    Code = donor.RelationshipProfileOption.Code,
                    Name = donor.RelationshipProfileOption.Name,
                    Color = donor.RelationshipProfileOption.Color,
                    SortOrder = donor.RelationshipProfileOption.SortOrder,
                    IsSystem = donor.RelationshipProfileOption.IsSystem,
                    IsActive = donor.RelationshipProfileOption.IsActive,
                },
                Tags = donor.TagAssignments
                    .OrderBy(tag => tag.DonorTag.Name)
                    .Select(tag => new DonorTagDto { Id = tag.DonorTagId, Name = tag.DonorTag.Name })
                    .ToList(),
                TotalDonated = _context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0,
                LastDonationAtUtc = _context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Max(donation => (DateTimeOffset?)donation.PaidAtUtc),
            });

        return await PaginatedResult<DonorListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
