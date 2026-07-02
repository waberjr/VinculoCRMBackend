using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

namespace VinculoBackend.Application.Donations.Commands.ConfirmDonation;

public record ConfirmDonationCommand(Guid Id, DateTimeOffset PaidAtUtc, string? Reference) : IRequest;

public sealed class ConfirmDonationCommandHandler : IRequestHandler<ConfirmDonationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public ConfirmDonationCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(ConfirmDonationCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donation = await _context.Donations
            .Include(entity => entity.StatusOption)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (donation is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Donation), request.Id.ToString());
        }

        if (donation.StatusOption.Code is not ("pending" or "overdue"))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(ConfirmDonationCommand.Id), "Apenas contribuicoes pendentes ou vencidas podem ser confirmadas."),
            ]);
        }

        donation.Confirm(
            await OptionLookup.RequiredIdAsync(_context, "DonationStatus", "Confirmed", cancellationToken),
            request.PaidAtUtc,
            request.Reference,
            donation.StatusOption.Code);

        _context.DonorTimelineEntries.Add(new DonorTimelineEntry
        {
            OrganizationId = donation.OrganizationId,
            DonorId = donation.DonorId,
            TypeOptionId = await OptionLookup.RequiredIdAsync(_context, "TimelineType", "Donation", cancellationToken),
            Title = "Contribuicao confirmada",
            Description = $"Pagamento confirmado no valor de {donation.Amount:C}.",
            OccurredAtUtc = DateTimeOffset.UtcNow,
            RelatedEntityType = nameof(Donation),
            RelatedEntityId = donation.Id,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
