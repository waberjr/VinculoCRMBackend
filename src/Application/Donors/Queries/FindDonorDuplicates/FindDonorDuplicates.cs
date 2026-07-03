using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;

namespace VinculoBackend.Application.Donors.Queries.FindDonorDuplicates;

public record FindDonorDuplicatesQuery : IRequest<IReadOnlyCollection<DonorDuplicateDto>>
{
    public string? Document { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? ExcludeDonorId { get; init; }
}

public sealed class FindDonorDuplicatesQueryHandler : IRequestHandler<FindDonorDuplicatesQuery, IReadOnlyCollection<DonorDuplicateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IBrazilianDocumentValidator _documentValidator;
    private readonly IOrganizationContext _organizationContext;

    public FindDonorDuplicatesQueryHandler(
        IApplicationDbContext context,
        IBrazilianDocumentValidator documentValidator,
        IOrganizationContext organizationContext)
    {
        _context = context;
        _documentValidator = documentValidator;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<DonorDuplicateDto>> Handle(FindDonorDuplicatesQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var document = NormalizeDocument(request.Document);
        var email = NormalizeEmail(request.Email);
        var phone = NormalizeDigits(request.Phone);

        if (document is null && email is null && phone is null)
        {
            return [];
        }

        var candidates = await _context.Donors
            .AsNoTracking()
            .Where(donor => request.ExcludeDonorId == null || donor.Id != request.ExcludeDonorId)
            .Where(donor =>
                (document != null && donor.Document == document) ||
                (email != null && (
                    (donor.Email != null && donor.Email.ToLower() == email) ||
                    donor.Emails.Any(item => item.Address.ToLower() == email))) ||
                (phone != null && (
                    donor.Phone != null ||
                    donor.WhatsApp != null ||
                    donor.Phones.Any())))
            .Select(donor => new
            {
                donor.Id,
                donor.FullName,
                donor.Document,
                donor.Email,
                donor.Phone,
                donor.WhatsApp,
                Phones = donor.Phones.Select(item => item.Number).ToList(),
                Emails = donor.Emails.Select(item => item.Address).ToList(),
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var matches = new List<string>();

                if (document is not null && candidate.Document == document)
                {
                    matches.Add("document");
                }

                if (email is not null &&
                    (NormalizeEmail(candidate.Email) == email ||
                     candidate.Emails.Any(item => NormalizeEmail(item) == email)))
                {
                    matches.Add("email");
                }

                if (phone is not null &&
                    (NormalizeDigits(candidate.Phone) == phone ||
                     NormalizeDigits(candidate.WhatsApp) == phone ||
                     candidate.Phones.Any(item => NormalizeDigits(item) == phone)))
                {
                    matches.Add("phone");
                }

                return new DonorDuplicateDto
                {
                    Id = candidate.Id,
                    FullName = candidate.FullName,
                    Document = candidate.Document,
                    Email = candidate.Email,
                    Phone = candidate.Phone,
                    MatchedFields = matches,
                };
            })
            .Where(candidate => candidate.MatchedFields.Count > 0)
            .ToList();
    }

    private string? NormalizeDocument(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return null;
        }

        return _documentValidator.Normalize(document);
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }
}
