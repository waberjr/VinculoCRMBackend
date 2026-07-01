namespace VinculoBackend.Application.Common.Models;

public sealed class OptionCategoryDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
