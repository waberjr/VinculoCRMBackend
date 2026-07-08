using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;

public sealed record CreateDocumentAttachmentCommand(string EntityType, Guid EntityId, string Title, string Url, string? Description) : IRequest<Guid>;

public sealed class CreateDocumentAttachmentCommandHandler : IRequestHandler<CreateDocumentAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateDocumentAttachmentCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateDocumentAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Url))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure("Document", "Informe entidade, titulo e link do documento.")]);
        }

        if (!await DocumentAttachmentEntityLookup.EntityExistsAsync(_context, request.EntityType, request.EntityId, cancellationToken))
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(request.EntityType, request.EntityId.ToString());
        }

        var organizationId = RequiredOrganization.From(_organizationContext);
        var document = new DocumentAttachment
        {
            OrganizationId = organizationId,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            Title = request.Title.Trim(),
            Url = request.Url.Trim(),
            Description = request.Description?.Trim(),
            CreatedByUserId = _user.Id,
        };

        _context.DocumentAttachments.Add(document);
        await AddTimelineEntryAsync(organizationId, request.EntityType, request.EntityId, document.Title, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }

    private async Task AddTimelineEntryAsync(Guid organizationId, string entityType, Guid entityId, string title, CancellationToken cancellationToken)
    {
        var donorId = await DocumentAttachmentEntityLookup.ResolveDonorIdAsync(_context, entityType, entityId, cancellationToken);
        if (donorId is null)
        {
            return;
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = organizationId,
            DonorId = donorId.Value,
            Type = TimelineEntryType.Note,
            Title = "Documento vinculado",
            Description = title,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = _user.Id,
            RelatedEntityType = entityType.Trim(),
            RelatedEntityId = entityId,
        });
    }
}
