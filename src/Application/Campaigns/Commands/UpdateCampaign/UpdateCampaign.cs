using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Campaigns.Commands.UpdateCampaign;

public record UpdateCampaignCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "Fundraising";
    public string? Channel { get; init; }
    public decimal? GoalAmount { get; init; }
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
    public string? Description { get; init; }
}

public sealed class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateCampaignCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var campaign = await _context.Campaigns.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (campaign is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Campaign), request.Id.ToString());
        }

        campaign.Update(
            request.Name,
            request.Description,
            SystemOptionMapper.Parse<CampaignType>(request.Type),
            string.IsNullOrWhiteSpace(request.Channel)
                ? null
                : SystemOptionMapper.Parse<CampaignChannel>(request.Channel),
            request.GoalAmount,
            request.StartDateUtc,
            request.EndDateUtc);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
