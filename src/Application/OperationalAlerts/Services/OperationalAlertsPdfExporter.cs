using VinculoBackend.Application.OperationalAlerts.Models;

namespace VinculoBackend.Application.OperationalAlerts.Services;

public interface IOperationalAlertsPdfExporter
{
    byte[] Generate(IReadOnlyCollection<OperationalAlertDto> alerts);
}
