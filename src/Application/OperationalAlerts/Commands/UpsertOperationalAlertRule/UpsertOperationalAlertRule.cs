using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.OperationalAlerts.Commands.UpsertOperationalAlertRule;

public sealed record UpsertOperationalAlertRuleCommand(
    string Source,
    bool IsEnabled,
    int WarningThreshold,
    int HighThreshold,
    int DueInHours,
    decimal? LowConversionThresholdPercent,
    bool IgnoreCancelledTasksForAutoResolution,
    string? AssignedUserId) : IRequest<Guid>;

public sealed class UpsertOperationalAlertRuleCommandHandler : IRequestHandler<UpsertOperationalAlertRuleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpsertOperationalAlertRuleCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(UpsertOperationalAlertRuleCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var source = request.Source.Trim();
        var rule = await _context.OperationalAlertRules.FirstOrDefaultAsync(entity => entity.Source == source, cancellationToken);
        if (rule is null)
        {
            rule = OperationalAlertRule.Create(
                organizationId,
                source,
                request.IsEnabled,
                request.WarningThreshold,
                request.HighThreshold,
                request.DueInHours,
                request.LowConversionThresholdPercent,
                request.IgnoreCancelledTasksForAutoResolution,
                request.AssignedUserId);
            _context.OperationalAlertRules.Add(rule);
        }
        else
        {
            rule.Update(request.IsEnabled, request.WarningThreshold, request.HighThreshold, request.DueInHours, request.LowConversionThresholdPercent, request.IgnoreCancelledTasksForAutoResolution, request.AssignedUserId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }
}
