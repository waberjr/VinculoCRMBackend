namespace VinculoBackend.Application.Donors.Models;

public sealed class DonorOperationalSegmentDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Count { get; init; }
    public string Tone { get; init; } = "neutral";
}

public sealed class DonorOperationalRiskDto
{
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Tone { get; init; } = "neutral";
    public string ActionLabel { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> QueryParams { get; init; } = new Dictionary<string, string>();
}
