namespace VinculoBackend.Application.Common.Models;

public sealed record FileUpload(
    string FileName,
    string ContentType,
    Stream Content,
    long Length);
