using VinculoBackend.Application.Campaigns.Models;

namespace VinculoBackend.Application.Campaigns.Services;

public interface ILandingPageBlockRulesPdfExporter
{
    byte[] Generate(IReadOnlyCollection<LandingPageBlockRuleDto> rules);
}
