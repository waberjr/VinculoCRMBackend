using System.Text;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageLeads;

namespace VinculoBackend.Application.Campaigns.Queries.ExportLandingPageLeads;

public sealed record ExportLandingPageLeadsQuery(
    string TargetType,
    Guid TargetId,
    string? Source = null,
    string? Status = null,
    DateTimeOffset? StartDateUtc = null,
    DateTimeOffset? EndDateUtc = null) : IRequest<LandingPageLeadsExportDto>;

public sealed record LandingPageLeadsExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportLandingPageLeadsQueryHandler : IRequestHandler<ExportLandingPageLeadsQuery, LandingPageLeadsExportDto>
{
    private readonly ISender _sender;

    public ExportLandingPageLeadsQueryHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<LandingPageLeadsExportDto> Handle(ExportLandingPageLeadsQuery request, CancellationToken cancellationToken)
    {
        var allItems = new List<LandingPageLeadDto>();
        var pageNumber = 1;
        var totalPages = 1;

        while (pageNumber <= totalPages)
        {
            var page = await _sender.Send(
                new GetLandingPageLeadsQuery(
                    request.TargetType,
                    request.TargetId,
                    request.Source,
                    request.Status,
                    request.StartDateUtc,
                    request.EndDateUtc,
                    pageNumber,
                    100),
                cancellationToken);

            allItems.AddRange(page.Items);
            totalPages = Math.Max(1, page.TotalPages);
            pageNumber++;
        }

        return new LandingPageLeadsExportDto(
            $"leads-landing-{request.TargetType}-{request.TargetId:N}.csv",
            "text/csv",
            GenerateCsv(allItems));
    }

    private static byte[] GenerateCsv(IEnumerable<LandingPageLeadDto> leads)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Nome,Email,Telefone,Origem,UTM Source,Valor prometido,Status,Cadastro");

        foreach (var lead in leads)
        {
            builder.AppendLine(string.Join(
                ',',
                Csv(lead.DonorName),
                Csv(lead.Email ?? string.Empty),
                Csv(lead.Phone ?? string.Empty),
                Csv(lead.Source),
                Csv(lead.UtmSource ?? string.Empty),
                lead.PromisedAmount?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                Csv(lead.DonationStatus ?? "Lead"),
                lead.CreatedAtUtc.ToString("O")));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
