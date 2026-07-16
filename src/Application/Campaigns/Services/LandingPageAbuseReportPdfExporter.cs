using VinculoBackend.Application.Campaigns.Models;

namespace VinculoBackend.Application.Campaigns.Services;

public interface ILandingPageAbuseReportPdfExporter
{
    byte[] Generate(LandingPageAbuseReportDto report);
}
