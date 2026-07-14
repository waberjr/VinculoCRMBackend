using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donors.Queries.GetDonorOperationalRisks;

public sealed record GetDonorOperationalRisksQuery(Guid DonorId) : IRequest<IReadOnlyCollection<DonorOperationalRiskDto>?>;

public sealed class GetDonorOperationalRisksQueryHandler : IRequestHandler<GetDonorOperationalRisksQuery, IReadOnlyCollection<DonorOperationalRiskDto>?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetDonorOperationalRisksQueryHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<DonorOperationalRiskDto>?> Handle(GetDonorOperationalRisksQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donor = await _context.Donors
            .AsNoTracking()
            .Where(entity => entity.Id == request.DonorId)
            .Select(entity => new
            {
                entity.Id,
                entity.FullName,
                entity.Document,
                entity.Email,
                entity.Phone,
                entity.AllowsCommunication,
                entity.DoNotContact,
                entity.DoNotContactReason,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (donor is null)
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        var today = now.Date;
        var lastDonationAt = await _context.Donations
            .AsNoTracking()
            .Where(donation =>
                donation.DonorId == request.DonorId &&
                donation.Status == DonationStatus.Confirmed &&
                donation.PaidAtUtc != null)
            .MaxAsync(donation => donation.PaidAtUtc, cancellationToken);

        var overdueDonationsCount = await _context.Donations
            .AsNoTracking()
            .CountAsync(donation =>
                donation.DonorId == request.DonorId &&
                (donation.Status == DonationStatus.Overdue ||
                    (donation.Status == DonationStatus.Pending && donation.ExpectedAtUtc < now)), cancellationToken);

        var interruptedRecurringCount = await _context.DonationPlans
            .AsNoTracking()
            .CountAsync(plan =>
                plan.DonorId == request.DonorId &&
                (plan.Status == DonationPlanStatus.Paused || plan.Status == DonationPlanStatus.Cancelled), cancellationToken);

        var overdueTasksCount = await _context.RelationshipTasks
            .AsNoTracking()
            .CountAsync(task =>
                task.DonorId == request.DonorId &&
                task.DueAtUtc != null &&
                task.DueAtUtc < today &&
                (task.Status == RelationshipTaskStatus.Open || task.Status == RelationshipTaskStatus.InProgress), cancellationToken);

        var risks = new List<DonorOperationalRiskDto>();

        if (donor.DoNotContact || !donor.AllowsCommunication)
        {
            risks.Add(Risk(
                "BlockedContact",
                "Contato bloqueado",
                donor.DoNotContactReason ?? "Revise consentimento antes de qualquer abordagem.",
                "red",
                "Editar cadastro",
                $"/doadores/{donor.Id}/editar"));
        }

        if (lastDonationAt is not null)
        {
            var daysSinceDonation = Math.Max(0, (int)Math.Floor((now - lastDonationAt.Value).TotalDays));
            if (daysSinceDonation >= 90)
            {
                risks.Add(Risk(
                    "NoDonation90Days",
                    "Sem doacao ha 90 dias",
                    $"Ultima doacao confirmada ha {daysSinceDonation} dias.",
                    "yellow",
                    "Criar tarefa",
                    "/tarefas/novo",
                    new Dictionary<string, string> { ["donorName"] = donor.FullName, ["segment"] = "NoDonation90Days" }));
            }
        }

        if (overdueDonationsCount > 0)
        {
            risks.Add(Risk(
                "OverdueDonation",
                "Cobranca vencida",
                $"{overdueDonationsCount} contribuicao pendente precisa de acompanhamento.",
                "red",
                "Ver cobrancas",
                "/contribuicoes",
                new Dictionary<string, string> { ["donor"] = donor.FullName, ["status"] = "Overdue" }));
        }

        if (interruptedRecurringCount > 0)
        {
            risks.Add(Risk(
                "InterruptedRecurring",
                "Recorrencia interrompida",
                $"{interruptedRecurringCount} plano pausado ou cancelado precisa de revisao.",
                "red",
                "Criar tarefa",
                "/tarefas/novo",
                new Dictionary<string, string> { ["donorName"] = donor.FullName, ["segment"] = "InterruptedRecurring" }));
        }

        if (overdueTasksCount > 0)
        {
            risks.Add(Risk(
                "OverdueTask",
                "Tarefa vencida",
                $"{overdueTasksCount} tarefa precisa de retorno.",
                "yellow",
                "Ver tarefas",
                "/tarefas",
                new Dictionary<string, string> { ["donorName"] = donor.FullName, ["due"] = "Overdue" }));
        }

        if (string.IsNullOrWhiteSpace(donor.Document) || (string.IsNullOrWhiteSpace(donor.Email) && string.IsNullOrWhiteSpace(donor.Phone)))
        {
            risks.Add(Risk(
                "DataQuality",
                "Cadastro incompleto",
                "Complete documento e pelo menos um canal de contato para melhorar a operacao.",
                "yellow",
                "Editar cadastro",
                $"/doadores/{donor.Id}/editar"));
        }

        return risks;
    }

    private static DonorOperationalRiskDto Risk(
        string code,
        string title,
        string description,
        string tone,
        string actionLabel,
        string route,
        IReadOnlyDictionary<string, string>? queryParams = null)
    {
        return new DonorOperationalRiskDto
        {
            Code = code,
            Title = title,
            Description = description,
            Tone = tone,
            ActionLabel = actionLabel,
            Route = route,
            QueryParams = queryParams ?? new Dictionary<string, string>(),
        };
    }
}
