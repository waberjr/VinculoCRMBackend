using VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalProductivity;

namespace VinculoBackend.Application.OperationalAlerts.Services;

public interface IOperationalProductivityPdfExporter
{
    byte[] Generate(OperationalProductivityDto productivity);
}
