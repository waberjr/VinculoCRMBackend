using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Communications.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Communications.Commands.CreateCommunicationCampaign;

public sealed record CreateCommunicationCampaignCommand(
    string Name,
    string Channel,
    string Audience,
    Guid? TemplateId,
    DateTimeOffset? ScheduledAtUtc,
    IReadOnlyCollection<Guid> DonorIds) : IRequest<Guid>;

public sealed class CreateCommunicationCampaignCommandHandler : IRequestHandler<CreateCommunicationCampaignCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly ICommunicationCampaignRecipientPlanner _recipientPlanner;

    public CreateCommunicationCampaignCommandHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        IUser user,
        ICommunicationCampaignRecipientPlanner recipientPlanner)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _recipientPlanner = recipientPlanner;
    }

    public async Task<Guid> Handle(CreateCommunicationCampaignCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

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
        var campaign = CommunicationCampaign.Create(
            organizationId,
            request.Name,
            channel,
            request.Audience,
            request.TemplateId,
            request.ScheduledAtUtc,
            _user.Id);

        _context.CommunicationCampaigns.Add(campaign);
        await _recipientPlanner.PlanRecipientsAsync(campaign, request.DonorIds, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return campaign.Id;
    }
}
