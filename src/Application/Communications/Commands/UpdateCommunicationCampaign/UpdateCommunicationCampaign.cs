using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Communications.Services;
using VinculoBackend.Domain.Entities;
using FluentValidation.Results;

namespace VinculoBackend.Application.Communications.Commands.UpdateCommunicationCampaign;

public sealed record UpdateCommunicationCampaignCommand(
    Guid Id,
    string Name,
    string Channel,
    string Audience,
    Guid? TemplateId,
    DateTimeOffset? ScheduledAtUtc,
    IReadOnlyCollection<Guid> DonorIds) : IRequest;

public sealed class UpdateCommunicationCampaignCommandHandler : IRequestHandler<UpdateCommunicationCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICommunicationCampaignRecipientPlanner _recipientPlanner;
    private readonly TimeProvider _timeProvider;

    public UpdateCommunicationCampaignCommandHandler(
        IApplicationDbContext context,
        ICommunicationCampaignRecipientPlanner recipientPlanner,
        TimeProvider timeProvider)
    {
        _context = context;
        _recipientPlanner = recipientPlanner;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateCommunicationCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.CommunicationCampaigns.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (campaign is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(CommunicationCampaign), request.Id.ToString());
        }

        if (request.TemplateId is not null)
        {
            var templateExists = await _context.CommunicationTemplates
                .AsNoTracking()
                .AnyAsync(template => template.Id == request.TemplateId.Value && template.IsActive, cancellationToken);

            if (!templateExists)
            {
                throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(CommunicationTemplate), request.TemplateId.Value.ToString());
            }
        }

        var channel = CommunicationChannelParser.Parse(request.Channel, nameof(request.Channel));
        campaign.UpdatePlan(
            request.Name,
            channel,
            request.Audience,
            request.TemplateId,
            request.ScheduledAtUtc);

        var recipients = await _context.CommunicationCampaignRecipients
            .Where(recipient => recipient.CommunicationCampaignId == campaign.Id)
            .ToListAsync(cancellationToken);
        var deletedAtUtc = _timeProvider.GetUtcNow();
        foreach (var recipient in recipients)
        {
            recipient.SoftDelete(deletedAtUtc);
        }

        campaign.ResetRecipientMetrics();
        await _recipientPlanner.PlanRecipientsAsync(campaign, request.DonorIds, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
