using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Donors.Models;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Donors.Queries.GetDonorNextBestAction;

public sealed record GetDonorNextBestActionQuery(Guid DonorId) : IRequest<DonorNextBestActionDto?>;

public sealed class GetDonorNextBestActionQueryHandler : IRequestHandler<GetDonorNextBestActionQuery, DonorNextBestActionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;
    private readonly TimeProvider _timeProvider;

    public GetDonorNextBestActionQueryHandler(
        IApplicationDbContext context,
        IOrganizationContext organizationContext,
        TimeProvider timeProvider)
    {
        _context = context;
        _organizationContext = organizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<DonorNextBestActionDto?> Handle(GetDonorNextBestActionQuery request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var donor = await _context.Donors
            .AsNoTracking()
            .Where(entity => entity.Id == request.DonorId)
            .Select(entity => new
            {
                entity.Id,
                entity.FullName,
                entity.Status,
                entity.AllowsCommunication,
                entity.DoNotContact,
                entity.DoNotContactReason,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (donor is null)
        {
            return null;
        }

        if (donor.DoNotContact || !donor.AllowsCommunication)
        {
            return Action(
                donor.Id,
                "Revisar consentimento",
                donor.DoNotContactReason ?? "O doador esta bloqueado ou sem consentimento para contato.",
                "Editar cadastro",
                $"/doadores/{donor.Id}/editar");
        }

        var now = _timeProvider.GetUtcNow();
        var overdueDonation = await _context.Donations
            .AsNoTracking()
            .AnyAsync(donation =>
                donation.DonorId == donor.Id &&
                (donation.Status == DonationStatus.Overdue ||
                    (donation.Status == DonationStatus.Pending && donation.ExpectedAtUtc < now)), cancellationToken);
        if (overdueDonation)
        {
            return Action(
                donor.Id,
                "Regularizar cobranca vencida",
                "Existe contribuicao pendente vencida para acompanhar.",
                "Ver cobrancas",
                "/contribuicoes",
                new Dictionary<string, string> { ["donor"] = donor.FullName, ["status"] = "Overdue" });
        }

        var openTask = await _context.RelationshipTasks
            .AsNoTracking()
            .Where(task =>
                task.DonorId == donor.Id &&
                (task.Status == RelationshipTaskStatus.Open || task.Status == RelationshipTaskStatus.InProgress))
            .OrderBy(task => task.DueAtUtc)
            .Select(task => task.Title)
            .FirstOrDefaultAsync(cancellationToken);
        if (openTask is not null)
        {
            return Action(
                donor.Id,
                "Executar tarefa aberta",
                openTask,
                "Ver tarefas",
                "/tarefas",
                new Dictionary<string, string> { ["donorName"] = donor.FullName });
        }

        var lastDonationAt = await _context.Donations
            .AsNoTracking()
            .Where(donation => donation.DonorId == donor.Id && donation.Status == DonationStatus.Confirmed && donation.PaidAtUtc != null)
            .MaxAsync(donation => donation.PaidAtUtc, cancellationToken);
        if (lastDonationAt is null || now - lastDonationAt.Value >= TimeSpan.FromDays(90))
        {
            return Action(
                donor.Id,
                donor.Status == DonorStatus.Lead ? "Converter lead" : "Reativar relacionamento",
                "Crie uma tarefa de contato com contexto de impacto e proxima campanha.",
                "Criar tarefa",
                "/tarefas/novo",
                new Dictionary<string, string> { ["donorName"] = donor.FullName, ["segment"] = donor.Status == DonorStatus.Lead ? "LeadsWithoutDonation" : "NoDonation90Days" });
        }

        return Action(
            donor.Id,
            "Agradecer e nutrir relacionamento",
            "Doador ativo sem pendencias imediatas. Planeje uma comunicacao de agradecimento ou impacto.",
            "Planejar comunicacao",
            "/comunicacao",
            new Dictionary<string, string> { ["donor"] = donor.FullName });
    }

    private static DonorNextBestActionDto Action(
        Guid donorId,
        string title,
        string description,
        string actionLabel,
        string route,
        IReadOnlyDictionary<string, string>? queryParams = null)
    {
        return new DonorNextBestActionDto
        {
            DonorId = donorId,
            Title = title,
            Description = description,
            ActionLabel = actionLabel,
            Route = route,
            QueryParams = queryParams ?? new Dictionary<string, string>(),
        };
    }
}
