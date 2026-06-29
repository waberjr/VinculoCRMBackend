using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;

namespace VinculoBackend.Application.Donors.Queries.GetDonors;

public record GetDonorsQuery : IRequest<PaginatedResult<DonorListItemDto>>
{
    public string? Search { get; init; }
    public Guid? StatusOptionId { get; init; }
    public Guid? PersonTypeOptionId { get; init; }
    public Guid? TagId { get; init; }
    public Guid? RelationshipProfileOptionId { get; init; }
    public Guid? PreferredContactChannelOptionId { get; init; }
    public string? AssignedUserId { get; init; }
    public bool? AllowsCommunication { get; init; }
    public bool? DoNotContact { get; init; }
    public string? Segment { get; init; }
    public string? Communication { get; init; }
    public string? DonationRange { get; init; }
    public string? DataQuality { get; init; }
    public string? DocumentStatus { get; init; }
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
                (donor.Phone != null && donor.Phone.Contains(search)) ||
                donor.Phones.Any(phone => phone.Number.Contains(search)) ||
                donor.Emails.Any(email => email.Address.ToLower().Contains(search)));
        }

        if (request.StatusOptionId is not null)
        {
            query = query.Where(donor => donor.StatusOptionId == request.StatusOptionId);
        }

        if (request.PersonTypeOptionId is not null)
        {
            query = query.Where(donor => donor.PersonTypeOptionId == request.PersonTypeOptionId);
        }

        if (request.RelationshipProfileOptionId is not null)
        {
            query = query.Where(donor => donor.RelationshipProfileOptionId == request.RelationshipProfileOptionId);
        }

        if (request.PreferredContactChannelOptionId is not null)
        {
            query = query.Where(donor => donor.PreferredContactChannelOptionId == request.PreferredContactChannelOptionId);
        }

        if (request.TagId is not null)
        {
            query = query.Where(donor => donor.TagAssignments.Any(tag => tag.DonorTagId == request.TagId));
        }

        if (!string.IsNullOrWhiteSpace(request.AssignedUserId))
        {
            query = query.Where(donor => donor.AssignedUserId == request.AssignedUserId);
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

        query = ApplyDocumentStatusFilter(query, request.DocumentStatus);
        query = ApplyCommunicationFilter(query, request.Communication);
        query = ApplySegmentFilter(query, request.Segment);
        query = ApplyDonationRangeFilter(query, request.DonationRange);
        query = ApplyDataQualityFilter(query, request.DataQuality);

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
                AcquisitionCampaignId = donor.AcquisitionCampaignId,
                AcquisitionCampaignName = donor.AcquisitionCampaign == null ? null : donor.AcquisitionCampaign.Name,
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
                Phones = donor.Phones
                    .OrderByDescending(phone => phone.IsPrimary)
                    .ThenBy(phone => phone.TypeOption.SortOrder)
                    .Select(phone => new DonorPhoneDto
                    {
                        Id = phone.Id,
                        TypeCode = phone.TypeOption.Code,
                        Number = phone.Number,
                        IsPrimary = phone.IsPrimary,
                        Type = new OptionDto
                        {
                            Id = phone.TypeOption.Id,
                            Category = phone.TypeOption.Category,
                            Code = phone.TypeOption.Code,
                            Name = phone.TypeOption.Name,
                            Color = phone.TypeOption.Color,
                            SortOrder = phone.TypeOption.SortOrder,
                            IsSystem = phone.TypeOption.IsSystem,
                            IsActive = phone.TypeOption.IsActive,
                        },
                    })
                    .ToList(),
                Emails = donor.Emails
                    .OrderByDescending(email => email.IsPrimary)
                    .ThenBy(email => email.TypeOption.SortOrder)
                    .Select(email => new DonorEmailDto
                    {
                        Id = email.Id,
                        TypeCode = email.TypeOption.Code,
                        Address = email.Address,
                        IsPrimary = email.IsPrimary,
                        Type = new OptionDto
                        {
                            Id = email.TypeOption.Id,
                            Category = email.TypeOption.Category,
                            Code = email.TypeOption.Code,
                            Name = email.TypeOption.Name,
                            Color = email.TypeOption.Color,
                            SortOrder = email.TypeOption.SortOrder,
                            IsSystem = email.TypeOption.IsSystem,
                            IsActive = email.TypeOption.IsActive,
                        },
                    })
                    .ToList(),
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

    private IQueryable<Domain.Entities.Donor> ApplyDocumentStatusFilter(IQueryable<Domain.Entities.Donor> query, string? documentStatus)
    {
        return documentStatus switch
        {
            "Valid" => query.Where(donor => donor.Document != null && donor.Document != ""),
            "Missing" => query.Where(donor => donor.Document == null || donor.Document == ""),
            _ => query,
        };
    }

    private IQueryable<Domain.Entities.Donor> ApplyCommunicationFilter(IQueryable<Domain.Entities.Donor> query, string? communication)
    {
        return communication switch
        {
            "Allowed" => query.Where(donor => donor.AllowsCommunication && !donor.DoNotContact),
            "NoConsent" => query.Where(donor => !donor.AllowsCommunication && !donor.DoNotContact),
            "DoNotContact" => query.Where(donor => donor.DoNotContact),
            _ => query,
        };
    }

    private IQueryable<Domain.Entities.Donor> ApplySegmentFilter(IQueryable<Domain.Entities.Donor> query, string? segment)
    {
        return segment switch
        {
            "Inactive" => query.Where(donor => donor.StatusOption.Code == "Inactive"),
            "AtRisk" => query.Where(donor => donor.StatusOption.Code == "AtRisk"),
            "LeadsWithoutDonation" => query.Where(donor =>
                donor.StatusOption.Code == "Lead" &&
                !_context.Donations.Any(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)),
            "NewDonors" => query.Where(donor => _context.Donations.Any(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)),
            "NoRecentContact" => query.Where(donor =>
                !_context.RelationshipTasks.Any(task =>
                    task.DonorId == donor.Id &&
                    (task.StatusOption.Code == "Open" || task.StatusOption.Code == "InProgress"))),
            _ => query,
        };
    }

    private IQueryable<Domain.Entities.Donor> ApplyDonationRangeFilter(IQueryable<Domain.Entities.Donor> query, string? donationRange)
    {
        return donationRange switch
        {
            "NeverDonated" => query.Where(donor => !_context.Donations.Any(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)),
            "UpTo1000" => query.Where(donor =>
                (_context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0) > 0 &&
                (_context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0) <= 1000),
            "From1000To5000" => query.Where(donor =>
                (_context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0) > 1000 &&
                (_context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0) <= 5000),
            "Above5000" => query.Where(donor =>
                (_context.Donations
                    .Where(donation => donation.DonorId == donor.Id && donation.PaidAtUtc != null)
                    .Sum(donation => (decimal?)donation.Amount) ?? 0) > 5000),
            _ => query,
        };
    }

    private IQueryable<Domain.Entities.Donor> ApplyDataQualityFilter(IQueryable<Domain.Entities.Donor> query, string? dataQuality)
    {
        return dataQuality switch
        {
            "MissingDocument" => query.Where(donor => donor.Document == null || donor.Document == ""),
            "MissingAddress" => query.Where(donor => donor.AddressLine1 == null || donor.AddressLine1 == ""),
            "LowScore" => query.Where(donor =>
                donor.Document == null || donor.Document == "" ||
                donor.Email == null || donor.Email == "" ||
                donor.Phone == null || donor.Phone == ""),
            _ => query,
        };
    }
}
