using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Commands.CreateCampaign;

public record CreateCampaignCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "Fundraising";
    public string? Channel { get; init; }
    public decimal? GoalAmount { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public string? Description { get; init; }
}

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;

    public CreateCampaignCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
    }

    public async Task<Guid> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);

        var campaign = Campaign.Create(
            organizationId,
            request.Name,
            request.Description,
            SystemOptionMapper.Parse<CampaignType>(request.Type),
            string.IsNullOrWhiteSpace(request.Channel)
                ? null
                : SystemOptionMapper.Parse<CampaignChannel>(request.Channel),
            request.GoalAmount,
            request.StartDateUtc,
            request.EndDateUtc,
            _user.Id);

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return campaign.Id;
    }
}
