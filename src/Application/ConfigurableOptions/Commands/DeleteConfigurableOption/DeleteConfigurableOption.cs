using VinculoBackend.Application.Common.Exceptions;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using FluentValidation.Results;

namespace VinculoBackend.Application.ConfigurableOptions.Commands.DeleteConfigurableOption;

public record DeleteConfigurableOptionCommand(Guid Id) : IRequest;

public sealed class DeleteConfigurableOptionCommandHandler : IRequestHandler<DeleteConfigurableOptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public DeleteConfigurableOptionCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(DeleteConfigurableOptionCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var entity = await _context.ConfigurableOptions.FirstOrDefaultAsync(option => option.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ConfigurableOptions), request.Id.ToString());
        }

        if (await IsOptionInUseAsync(entity.Id, cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
            [
                new ValidationFailure(nameof(DeleteConfigurableOptionCommand.Id), "Nao e possivel excluir uma opcao relacionada a registros do sistema."),
            ]);
        }

        _context.ConfigurableOptions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> IsOptionInUseAsync(Guid optionId, CancellationToken cancellationToken)
    {
        return await _context.Donors.AsNoTracking().AnyAsync(donor =>
                donor.PersonTypeOptionId == optionId ||
                donor.StatusOptionId == optionId ||
                donor.SourceOptionId == optionId ||
                donor.RelationshipProfileOptionId == optionId ||
                donor.PreferredContactChannelOptionId == optionId,
                cancellationToken)
            || await _context.DonorPhones.AsNoTracking().AnyAsync(phone => phone.TypeOptionId == optionId, cancellationToken)
            || await _context.DonorEmails.AsNoTracking().AnyAsync(email => email.TypeOptionId == optionId, cancellationToken)
            || await _context.Donations.AsNoTracking().AnyAsync(donation =>
                donation.TypeOptionId == optionId ||
                donation.StatusOptionId == optionId ||
                donation.PaymentMethodOptionId == optionId,
                cancellationToken)
            || await _context.DonationPlans.AsNoTracking().AnyAsync(plan =>
                plan.PreferredPaymentMethodOptionId == optionId ||
                plan.StatusOptionId == optionId,
                cancellationToken)
            || await _context.Campaigns.AsNoTracking().AnyAsync(campaign =>
                campaign.TypeOptionId == optionId ||
                campaign.StatusOptionId == optionId ||
                campaign.ChannelOptionId == optionId,
                cancellationToken)
            || await _context.RelationshipTasks.AsNoTracking().AnyAsync(task =>
                task.TypeOptionId == optionId ||
                task.PriorityOptionId == optionId ||
                task.StatusOptionId == optionId ||
                task.ContactOutcomeOptionId == optionId,
                cancellationToken)
            || await _context.DonorTimelineEntries.AsNoTracking().AnyAsync(entry => entry.TypeOptionId == optionId, cancellationToken);
    }
}
