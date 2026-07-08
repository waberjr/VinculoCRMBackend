namespace VinculoBackend.Application.Common.Options;

public sealed class DocumentUploadOptions
{
    public const string SectionName = "DocumentUpload";

    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; init; } =
    [
        ".pdf",
        ".png",
        ".jpg",
        ".jpeg",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".csv",
        ".txt",
    ];

    public string[] AllowedContentTypes { get; init; } =
    [
        "application/pdf",
        "image/png",
        "image/jpeg",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/csv",
        "text/plain",
    ];
}
