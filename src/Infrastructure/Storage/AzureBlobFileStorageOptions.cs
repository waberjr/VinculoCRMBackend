namespace VinculoBackend.Infrastructure.Storage;

public sealed class AzureBlobFileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string ConnectionString { get; init; } = "UseDevelopmentStorage=true";

    public string ContainerName { get; init; } = "vinculo-documents";
}
