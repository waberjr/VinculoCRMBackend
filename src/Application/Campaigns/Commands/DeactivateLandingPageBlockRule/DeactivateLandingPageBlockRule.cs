using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.DeactivateLandingPageBlockRule;

public sealed record DeactivateLandingPageBlockRuleCommand(Guid Id) : IRequest;

public sealed class DeactivateLandingPageBlockRuleCommandHandler : IRequestHandler<DeactivateLandingPageBlockRuleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateLandingPageBlockRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateLandingPageBlockRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.LandingPageBlockRules.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (rule is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(LandingPageBlockRule), request.Id.ToString());
        }

        rule.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
