using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Organizations.Services;

public sealed class OrganizationDefaultsService : IOrganizationDefaultsService
{
    private readonly IApplicationDbContext _context;

    public OrganizationDefaultsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureDefaultsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var existingOptions = await _context.ConfigurableOptions
            .IgnoreQueryFilters()
            .Where(option => option.OrganizationId == organizationId)
            .Select(option => new { option.Category, option.Code })
            .ToListAsync(cancellationToken);

        foreach (var option in DefaultOptions())
        {
            if (existingOptions.Any(existing => existing.Category == option.Category && existing.Code == option.Code))
            {
                continue;
            }

            _context.ConfigurableOptions.Add(new ConfigurableOption
            {
                OrganizationId = organizationId,
                Category = option.Category,
                Code = option.Code,
                Name = option.Name,
                Color = option.Color,
                SortOrder = option.SortOrder,
                IsSystem = true,
                IsActive = true,
            });
        }

        var existingTagNames = await _context.DonorTags
            .IgnoreQueryFilters()
            .Where(tag => tag.OrganizationId == organizationId)
            .Select(tag => tag.Name.ToLower())
            .ToListAsync(cancellationToken);

        foreach (var tag in DefaultTags())
        {
            if (existingTagNames.Contains(tag.Name.ToLower()))
            {
                continue;
            }

            _context.DonorTags.Add(new DonorTag
            {
                OrganizationId = organizationId,
                Name = tag.Name,
                Description = tag.Description,
                IsActive = true,
            });
        }
    }

    private static IReadOnlyCollection<DefaultOption> DefaultOptions() =>
    [
        Option("DonorPersonType", "Individual", "Pessoa fisica", 1),
        Option("DonorPersonType", "Company", "Pessoa juridica", 2),
        Option("DonorStatus", "Lead", "Lead", 1, "blue"),
        Option("DonorStatus", "Active", "Ativo", 2, "green"),
        Option("DonorStatus", "Inactive", "Inativo", 3, "neutral"),
        Option("DonorStatus", "AtRisk", "Em risco", 4, "yellow"),
        Option("DonorStatus", "DoNotContact", "Nao contatar", 5, "red"),
        Option("RelationshipProfile", "New", "Novo", 1),
        Option("RelationshipProfile", "Recurring", "Recorrente", 2),
        Option("RelationshipProfile", "Major", "Grande doador", 3),
        Option("RelationshipProfile", "Lapsed", "Inativo", 4),
        Option("RelationshipProfile", "Reactivated", "Reativado", 5),
        Option("RelationshipProfile", "Prospect", "Prospect", 6),
        Option("DonorSource", "Manual", "Manual", 1),
        Option("DonorSource", "Referral", "Indicacao", 2),
        Option("DonorSource", "Phone", "Telefone", 3),
        Option("DonorSource", "WhatsApp", "WhatsApp", 4),
        Option("DonorSource", "Email", "E-mail", 5),
        Option("DonorSource", "SocialMedia", "Redes sociais", 6),
        Option("DonorSource", "Website", "Website", 7),
        Option("DonorSource", "Event", "Evento", 8),
        Option("DonorSource", "Import", "Importacao", 9),
        Option("DonorSource", "Other", "Outro", 10),
        Option("ContactChannel", "Phone", "Telefone", 1),
        Option("ContactChannel", "WhatsApp", "WhatsApp", 2),
        Option("ContactChannel", "Email", "E-mail", 3),
        Option("ContactChannel", "Other", "Outro", 4),
        Option("DonationType", "OneTime", "Pontual", 1),
        Option("DonationType", "Recurring", "Recorrente", 2),
        Option("DonationType", "Pledge", "Promessa", 3),
        Option("DonationStatus", "Pending", "Pendente", 1, "yellow"),
        Option("DonationStatus", "Confirmed", "Confirmada", 2, "green"),
        Option("DonationStatus", "Overdue", "Vencida", 3, "red"),
        Option("DonationStatus", "Cancelled", "Cancelada", 4),
        Option("DonationStatus", "Refunded", "Estornada", 5, "yellow"),
        Option("PaymentMethod", "Pix", "Pix", 1),
        Option("PaymentMethod", "Boleto", "Boleto", 2),
        Option("PaymentMethod", "CreditCard", "Cartao de credito", 3),
        Option("PaymentMethod", "BankTransfer", "Transferencia bancaria", 4),
        Option("PaymentMethod", "Cash", "Dinheiro", 5),
        Option("PaymentMethod", "Other", "Outro", 6),
        Option("TaskType", "Call", "Ligacao", 1),
        Option("TaskType", "WhatsApp", "WhatsApp", 2),
        Option("TaskType", "PaymentReminder", "Lembrete de pagamento", 3),
        Option("TaskType", "Email", "E-mail", 4),
        Option("TaskType", "FollowUp", "Follow-up", 5),
        Option("TaskType", "ThankYou", "Agradecimento", 6),
        Option("TaskType", "DataUpdate", "Atualizacao cadastral", 7),
        Option("TaskType", "Other", "Outra", 8),
        Option("TaskPriority", "Low", "Baixa", 1),
        Option("TaskPriority", "Medium", "Media", 2),
        Option("TaskPriority", "High", "Alta", 3),
        Option("TaskPriority", "Urgent", "Urgente", 4),
        Option("TaskStatus", "Open", "Aberta", 1, "blue"),
        Option("TaskStatus", "InProgress", "Em andamento", 2, "yellow"),
        Option("TaskStatus", "Completed", "Concluida", 3, "green"),
        Option("TaskStatus", "Cancelled", "Cancelada", 4),
        Option("ContactOutcome", "Reached", "Contato realizado", 1),
        Option("ContactOutcome", "NoAnswer", "Nao atendeu", 2),
        Option("ContactOutcome", "InvalidContact", "Contato invalido", 3),
        Option("ContactOutcome", "RequestedCallback", "Retorno solicitado", 4),
        Option("ContactOutcome", "DonationConfirmed", "Doacao confirmada", 5),
        Option("ContactOutcome", "NotInterested", "Sem interesse", 6),
        Option("ContactOutcome", "DoNotContact", "Nao contatar", 7),
        Option("ContactOutcome", "Other", "Outro", 8),
        Option("CampaignType", "Acquisition", "Captacao", 1),
        Option("CampaignType", "Fundraising", "Captacao", 2),
        Option("CampaignType", "Retention", "Retencao", 3),
        Option("CampaignType", "Reactivation", "Reativacao", 4),
        Option("CampaignType", "Emergency", "Emergencial", 5),
        Option("CampaignType", "Other", "Outra", 6),
        Option("CampaignStatus", "Draft", "Rascunho", 1),
        Option("CampaignStatus", "Active", "Ativa", 2),
        Option("CampaignStatus", "Completed", "Concluida", 3),
        Option("CampaignStatus", "Cancelled", "Cancelada", 4),
        Option("CampaignChannel", "Mixed", "Multicanal", 1),
        Option("CampaignChannel", "Phone", "Telefone", 2),
        Option("CampaignChannel", "WhatsApp", "WhatsApp", 3),
        Option("CampaignChannel", "Email", "E-mail", 4),
        Option("CampaignChannel", "SocialMedia", "Redes sociais", 5),
        Option("CampaignChannel", "InPerson", "Presencial", 6),
        Option("CampaignChannel", "Other", "Outro", 7),
        Option("DonationPlanStatus", "Active", "Ativa", 1, "green"),
        Option("DonationPlanStatus", "Paused", "Pausada", 2, "yellow"),
        Option("DonationPlanStatus", "Cancelled", "Cancelada", 3),
        Option("TimelineType", "Note", "Nota", 1),
        Option("TimelineType", "Donation", "Contribuicao", 2, "green"),
        Option("TimelineType", "Task", "Tarefa", 3, "blue"),
        Option("TimelineType", "Contact", "Contato", 4, "yellow"),
        Option("PhoneType", "Mobile", "Celular", 1),
        Option("PhoneType", "WhatsApp", "WhatsApp", 2),
        Option("PhoneType", "Home", "Residencial", 3),
        Option("PhoneType", "Work", "Comercial", 4),
        Option("EmailType", "Personal", "Pessoal", 1),
        Option("EmailType", "Work", "Comercial", 2),
        Option("EmailType", "Billing", "Cobranca", 3),
    ];

    private static IReadOnlyCollection<DefaultTag> DefaultTags() =>
    [
        new("Recorrente", "Doador com relacionamento recorrente."),
        new("Alto valor", "Doador com historico relevante de contribuicoes."),
    ];

    private static DefaultOption Option(string category, string code, string name, int sortOrder, string? color = null) =>
        new(category, code, name, sortOrder, color);

    private sealed record DefaultOption(string Category, string Code, string Name, int SortOrder, string? Color);

    private sealed record DefaultTag(string Name, string Description);
}
