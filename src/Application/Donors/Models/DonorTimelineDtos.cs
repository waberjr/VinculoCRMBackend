namespace VinculoBackend.Application.Donors.Models;

public sealed class DonorTimelineEntryDto
{
    public Guid Id { get; init; }
    public string DonorName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string Tone { get; init; } = "neutral";
}

public sealed class DonorTimelineResponseDto
{
    public IReadOnlyCollection<DonorTimelineEntryDto> Items { get; init; } = [];
}
