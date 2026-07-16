using System.Text;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageBlockRules;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.Campaigns.Queries.ExportLandingPageBlockRules;

public sealed record ExportLandingPageBlockRulesQuery(
    string Format,
    string? TargetType = null,
    Guid? TargetId = null,
    bool IncludeInactive = true,
    bool IncludeExpired = true,
    string? Source = null,
    string? FingerprintHash = null,
    bool? Active = null,
    bool? Expired = null) : IRequest<LandingPageBlockRulesExportDto>;

public sealed record LandingPageBlockRulesExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportLandingPageBlockRulesQueryHandler : IRequestHandler<ExportLandingPageBlockRulesQuery, LandingPageBlockRulesExportDto>
{
    private readonly ISender _sender;
    private readonly ILandingPageBlockRulesPdfExporter _pdfExporter;

    public ExportLandingPageBlockRulesQueryHandler(ISender sender, ILandingPageBlockRulesPdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<LandingPageBlockRulesExportDto> Handle(ExportLandingPageBlockRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _sender.Send(new GetLandingPageBlockRulesQuery(
            request.TargetType,
            request.TargetId,
            request.IncludeInactive,
            request.IncludeExpired,
            request.Source,
            request.FingerprintHash,
            request.Active,
            request.Expired), cancellationToken);

        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new LandingPageBlockRulesExportDto("captacao-regras-bloqueio.pdf", "application/pdf", _pdfExporter.Generate(rules))
            : new LandingPageBlockRulesExportDto("captacao-regras-bloqueio.csv", "text/csv", GenerateCsv(rules));
    }

    private static byte[] GenerateCsv(IEnumerable<LandingPageBlockRuleDto> rules)
    {
        var builder = new StringBuilder();
        builder.AppendLine("CriadaEm,Tipo,Landing,ID,Fingerprint,Origem,Motivo,ExpiraEm,Ativa,Expirada");
        foreach (var rule in rules)
        {
            builder.AppendLine(string.Join(
                ',',
                rule.CreatedAtUtc.ToString("O"),
                Csv(rule.TargetType),
                Csv(rule.TargetName),
                rule.TargetId,
                Csv(rule.FingerprintHash ?? string.Empty),
                Csv(rule.Source ?? string.Empty),
                Csv(rule.Reason ?? string.Empty),
                rule.ExpiresAtUtc?.ToString("O") ?? string.Empty,
                rule.IsActive ? "Sim" : "Nao",
                rule.IsExpired ? "Sim" : "Nao"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
