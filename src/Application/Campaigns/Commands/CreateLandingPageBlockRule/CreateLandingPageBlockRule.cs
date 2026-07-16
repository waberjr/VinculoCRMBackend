using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Campaigns.Commands.CreateLandingPageBlockRule;

public sealed record CreateLandingPageBlockRuleCommand(
    string TargetType,
    Guid TargetId,
    string? FingerprintHash,
    string? Source,
    string? Reason) : IRequest<Guid>;

public sealed class CreateLandingPageBlockRuleCommandHandler : IRequestHandler<CreateLandingPageBlockRuleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public CreateLandingPageBlockRuleCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateLandingPageBlockRuleCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var targetType = NormalizeTargetType(request.TargetType);
        var fingerprintHash = TrimToNull(request.FingerprintHash);
        var source = TrimToNull(request.Source);
        if (fingerprintHash is null && source is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException(
                [new FluentValidation.Results.ValidationFailure(nameof(CreateLandingPageBlockRuleCommand.FingerprintHash), "Informe fingerprint ou origem para bloquear.")]);
        }

        var existing = await _context.LandingPageBlockRules
            .FirstOrDefaultAsync(rule =>
                rule.TargetType == targetType &&
                rule.TargetId == request.TargetId &&
                rule.FingerprintHash == fingerprintHash &&
                rule.Source == source,
                cancellationToken);

        if (existing is not null)
        {
            existing.IsActive = true;
            existing.Reason = TrimToNull(request.Reason) ?? existing.Reason;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var rule = new LandingPageBlockRule
        {
            OrganizationId = organizationId,
            TargetType = targetType,
            TargetId = request.TargetId,
            FingerprintHash = fingerprintHash,
            Source = source,
            Reason = TrimToNull(request.Reason),
            CreatedAtUtc = _timeProvider.GetUtcNow(),
            CreatedByUserId = _user.Id,
        };
        _context.LandingPageBlockRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }

    private static string NormalizeTargetType(string targetType)
        => targetType.Trim().ToLowerInvariant() is "campaign" or "project" ? targetType.Trim().ToLowerInvariant() : throw new global::VinculoBackend.Domain.Exceptions.DomainValidationException("Tipo de landing invalido.");

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
