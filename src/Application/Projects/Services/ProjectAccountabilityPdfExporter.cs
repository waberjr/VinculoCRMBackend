using VinculoBackend.Application.ImpactProjects.Models;

namespace VinculoBackend.Application.ImpactProjects.Services;

public interface IProjectAccountabilityPdfExporter
{
    byte[] Generate(ProjectAccountabilityDto report);
}
