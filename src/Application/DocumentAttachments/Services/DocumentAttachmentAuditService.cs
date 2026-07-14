using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.DocumentAttachments.Services;

public interface IDocumentAttachmentAuditService
{
    Task RecordAsync(DocumentAttachment document, string action, string timelineTitle, CancellationToken cancellationToken);
}

public sealed class DocumentAttachmentAuditService : IDocumentAttachmentAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DocumentAttachmentAuditService(IApplicationDbContext context, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task RecordAsync(DocumentAttachment document, string action, string timelineTitle, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        _context.DocumentAttachmentAuditEntries.Add(new DocumentAttachmentAuditEntry
        {
            OrganizationId = document.OrganizationId,
            DocumentAttachmentId = document.Id,
            EntityType = document.EntityType,
            EntityId = document.EntityId,
            Action = action,
            Title = document.Title,
            CreatedByUserId = _user.Id,
            OccurredAtUtc = now,
        });

        var donorId = await DocumentAttachmentEntityLookup.ResolveDonorIdAsync(_context, document.EntityType, document.EntityId, cancellationToken);
        if (donorId is null)
        {
            return;
        }

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = document.OrganizationId,
            DonorId = donorId.Value,
            Type = TimelineEntryType.Note,
            Title = timelineTitle,
            Description = document.Title,
            OccurredAtUtc = now,
            CreatedByUserId = _user.Id,
            RelatedEntityType = document.EntityType,
            RelatedEntityId = document.EntityId,
        });
    }
}
