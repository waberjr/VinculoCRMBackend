using VinculoBackend.Application.Campaigns.Models;

namespace VinculoBackend.Application.Campaigns.Services;

public interface ICampaignReportPdfExporter
{
    byte[] Generate(CampaignReportDto report);
}
