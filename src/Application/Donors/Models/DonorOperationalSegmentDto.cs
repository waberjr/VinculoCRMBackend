namespace VinculoBackend.Application.Donors.Models;

public sealed class DonorOperationalSegmentDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Count { get; init; }
    public string Tone { get; init; } = "neutral";
}
