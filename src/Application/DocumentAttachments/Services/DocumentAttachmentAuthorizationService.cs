using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.DocumentAttachments.Services;

public interface IDocumentAttachmentAuthorizationService
{
    bool CanDelete(DocumentAttachment document);
}

public sealed class DocumentAttachmentAuthorizationService : IDocumentAttachmentAuthorizationService
{
    private readonly IUser _user;

    public DocumentAttachmentAuthorizationService(IUser user)
    {
        _user = user;
    }

    public bool CanDelete(DocumentAttachment document)
    {
        var roles = _user.Roles ?? [];
        if (roles.Contains(Roles.SystemAdministrator) || roles.Contains(Roles.Administrator) || roles.Contains(Roles.Manager))
        {
            return true;
        }

        return roles.Contains(Roles.Agent) &&
            document.CreatedByUserId == _user.Id &&
            document.EntityType is "Donor" or "Donation";
    }
}
