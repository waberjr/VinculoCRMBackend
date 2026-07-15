using VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;

namespace VinculoBackend.Application.Campaigns.Services;

public interface ILandingPagePerformancePdfExporter
{
    byte[] Generate(LandingPagePerformanceDto report);
}
