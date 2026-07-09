using FluentValidation.Results;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Organizations.Services;

public interface IOrganizationLogoService
{
    Task<string> StoreAsync(Guid organizationId, FileUpload logo, CancellationToken cancellationToken);
}

public sealed class OrganizationLogoService : IOrganizationLogoService
{
    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".webp", ".svg"];
    private static readonly string[] AllowedContentTypes = ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];
    private const long MaxSizeBytes = 2 * 1024 * 1024;

    private readonly IFileStorageService _fileStorage;

    public OrganizationLogoService(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<string> StoreAsync(Guid organizationId, FileUpload logo, CancellationToken cancellationToken)
    {
        Validate(logo);

        var storedFile = await _fileStorage.StoreAsync(
            new StoreFileRequest(
                organizationId,
                "OrganizationLogo",
                organizationId,
                logo.FileName,
                logo.ContentType,
                logo.Content,
                logo.Length),
            cancellationToken);

        return storedFile.InternalUri;
    }

    private static void Validate(FileUpload logo)
    {
        var failures = new List<ValidationFailure>();
        if (logo.Length <= 0)
        {
            failures.Add(new ValidationFailure("Logo", "Informe uma imagem valida."));
        }

        if (logo.Length > MaxSizeBytes)
        {
            failures.Add(new ValidationFailure("Logo", "A logo deve ter no maximo 2 MB."));
        }

        var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure("LogoFileName", "Formato de logo nao permitido."));
        }

        if (string.IsNullOrWhiteSpace(logo.ContentType) ||
            !AllowedContentTypes.Contains(logo.ContentType.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(new ValidationFailure("LogoContentType", "Conteudo da logo nao permitido."));
        }

        if (failures.Count > 0)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException(failures);
        }
    }
}
