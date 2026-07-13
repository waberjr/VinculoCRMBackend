using System.Text;
using VinculoBackend.Application.ImpactProjects.Services;
using VinculoBackend.Application.ImpactProjects.Queries.GetProjectAccountability;

namespace VinculoBackend.Application.ImpactProjects.Queries.ExportProjectAccountability;

public sealed record ExportProjectAccountabilityQuery(
    Guid ProjectId,
    string Format,
    Guid? CampaignId,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc) : IRequest<ProjectAccountabilityExportDto?>;

public sealed record ProjectAccountabilityExportDto(string FileName, string ContentType, byte[] Content);

public sealed class ExportProjectAccountabilityQueryHandler : IRequestHandler<ExportProjectAccountabilityQuery, ProjectAccountabilityExportDto?>
{
    private readonly ISender _sender;
    private readonly IProjectAccountabilityPdfExporter _pdfExporter;

    public ExportProjectAccountabilityQueryHandler(ISender sender, IProjectAccountabilityPdfExporter pdfExporter)
    {
        _sender = sender;
        _pdfExporter = pdfExporter;
    }

    public async Task<ProjectAccountabilityExportDto?> Handle(ExportProjectAccountabilityQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetProjectAccountabilityQuery(request.ProjectId, request.CampaignId, request.StartDateUtc, request.EndDateUtc), cancellationToken);
        if (report is null)
        {
            return null;
        }

        return request.Format.Trim().ToLowerInvariant() == "pdf"
            ? new ProjectAccountabilityExportDto($"{report.ProjectName}-prestacao.pdf", "application/pdf", _pdfExporter.Generate(report))
            : new ProjectAccountabilityExportDto($"{report.ProjectName}-prestacao.csv", "text/csv", GenerateCsv(report));
    }

    private static byte[] GenerateCsv(Models.ProjectAccountabilityDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Projeto,Meta,Arrecadado,Doadores,Doacoes");
        builder.AppendLine($"{Csv(report.ProjectName)},{report.GoalAmount},{report.RaisedAmount},{report.DonorsCount},{report.DonationsCount}");
        builder.AppendLine("Campanha filtrada,Inicio,Fim");
        builder.AppendLine($"{Csv(report.FilterCampaignName ?? "Todas")},{report.FilterStartDateUtc:yyyy-MM-dd},{report.FilterEndDateUtc:yyyy-MM-dd}");
        builder.AppendLine();
        builder.AppendLine("Campanha,Arrecadado,Doadores,Doacoes,Ticket medio,Participacao");
        foreach (var campaign in report.Campaigns)
        {
            builder.AppendLine($"{Csv(campaign.Name)},{campaign.RaisedAmount},{campaign.DonorsCount},{campaign.DonationsCount},{campaign.AverageDonationAmount},{campaign.SharePercentage}%");
        }

        builder.AppendLine();
        builder.AppendLine("Doador,Campanha,Valor,Pagamento,Recibo,Referencia");
        foreach (var donation in report.Donations)
        {
            builder.AppendLine($"{Csv(donation.DonorName)},{Csv(donation.CampaignName ?? "Sem campanha")},{donation.Amount},{donation.PaidAtUtc:yyyy-MM-dd},{Csv(donation.ReceiptNumber ?? "")},{Csv(donation.Reference ?? "")}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
