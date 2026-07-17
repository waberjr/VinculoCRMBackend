using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.OperationalAlerts.Models;

namespace VinculoBackend.Application.OperationalAlerts.Queries.GetOperationalAlertRules;

public sealed record GetOperationalAlertRulesQuery : IRequest<IReadOnlyCollection<OperationalAlertRuleDto>>;

public sealed class GetOperationalAlertRulesQueryHandler : IRequestHandler<GetOperationalAlertRulesQuery, IReadOnlyCollection<OperationalAlertRuleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public GetOperationalAlertRulesQueryHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<IReadOnlyCollection<OperationalAlertRuleDto>> Handle(GetOperationalAlertRulesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var existing = await _context.OperationalAlertRules
            .AsNoTracking()
            .Select(rule => new OperationalAlertRuleDto
            {
                Id = rule.Id,
                Source = rule.Source,
                IsEnabled = rule.IsEnabled,
                WarningThreshold = rule.WarningThreshold,
                HighThreshold = rule.HighThreshold,
                DueInHours = rule.DueInHours,
                LowConversionThresholdPercent = rule.LowConversionThresholdPercent,
                AssignedUserId = rule.AssignedUserId,
            })
            .ToArrayAsync(cancellationToken);

        if (existing.Length > 0)
        {
            return existing;
        }

        return DefaultRules(organizationId).Select(rule => new OperationalAlertRuleDto
        {
            Id = rule.Id,
            Source = rule.Source,
            IsEnabled = rule.IsEnabled,
            WarningThreshold = rule.WarningThreshold,
            HighThreshold = rule.HighThreshold,
            DueInHours = rule.DueInHours,
            LowConversionThresholdPercent = rule.LowConversionThresholdPercent,
            AssignedUserId = rule.AssignedUserId,
        }).ToArray();
    }

    public static IReadOnlyCollection<Domain.Entities.OperationalAlertRule> DefaultRules(Guid organizationId) =>
    [
        Domain.Entities.OperationalAlertRule.Create(organizationId, "DonorRisk", true, 1, 10, 24, null, null),
        Domain.Entities.OperationalAlertRule.Create(organizationId, "OverdueTasks", true, 1, 10, 8, null, null),
        Domain.Entities.OperationalAlertRule.Create(organizationId, "PendingReceipts", true, 1, 10, 48, null, null),
        Domain.Entities.OperationalAlertRule.Create(organizationId, "CampaignLowConversion", true, 30, 100, 48, 5, null),
        Domain.Entities.OperationalAlertRule.Create(organizationId, "ProjectLowConversion", true, 30, 100, 48, 5, null),
    ];
}
