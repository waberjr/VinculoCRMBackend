using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.DocumentAttachments;

internal static class DocumentAttachmentEntityLookup
{
    public static async Task<bool> EntityExistsAsync(
        IApplicationDbContext context,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        return entityType.Trim() switch
        {
            "Donor" => await context.Donors.AsNoTracking().AnyAsync(entity => entity.Id == entityId, cancellationToken),
            "Donation" => await context.Donations.AsNoTracking().AnyAsync(entity => entity.Id == entityId, cancellationToken),
            "Receipt" => await context.Receipts.AsNoTracking().AnyAsync(entity => entity.Id == entityId, cancellationToken),
            "Project" => await context.Projects.AsNoTracking().AnyAsync(entity => entity.Id == entityId, cancellationToken),
            _ => false,
        };
    }

    public static async Task<Guid?> ResolveDonorIdAsync(
        IApplicationDbContext context,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        return entityType.Trim() switch
        {
            "Donor" => await context.Donors.AsNoTracking()
                .Where(entity => entity.Id == entityId)
                .Select(entity => (Guid?)entity.Id)
                .FirstOrDefaultAsync(cancellationToken),
            "Donation" => await context.Donations.AsNoTracking()
                .Where(entity => entity.Id == entityId)
                .Select(entity => (Guid?)entity.DonorId)
                .FirstOrDefaultAsync(cancellationToken),
            "Receipt" => await context.Receipts.AsNoTracking()
                .Where(entity => entity.Id == entityId)
                .Select(entity => (Guid?)entity.DonorId)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null,
        };
    }
}
