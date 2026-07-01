using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

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
            .ToListAsync(cancellationToken);

        foreach (var option in DefaultOptions())
        {
            var category = option.Category.ToString();
            var existingCodes = existingOptions
                .Where(existing => existing.Category == category)
                .Select(existing => existing.Code)
                .ToList();
            var baseCode = ConfigurableOptionCode.FromName(option.Code);
            if (existingCodes.Contains(baseCode, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var existingLegacyOption = existingOptions.FirstOrDefault(existing =>
                existing.Category == category &&
                string.Equals(existing.Code, option.Code, StringComparison.OrdinalIgnoreCase));

            if (existingLegacyOption is not null)
            {
                existingLegacyOption.Code = baseCode;
                continue;
            }

            var code = ConfigurableOptionCode.CreateUnique(option.Code, existingCodes);

            if (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _context.ConfigurableOptions.Add(new ConfigurableOption
            {
                OrganizationId = organizationId,
                Category = category,
                Code = code,
                Name = option.Name,
                Color = option.Color,
                SortOrder = option.SortOrder,
                IsSystem = true,
                IsActive = true,
            });

            existingOptions.Add(new ConfigurableOption
            {
                OrganizationId = organizationId,
                Category = category,
                Code = code,
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
        Option(ConfigurableOptionCategory.DonorPersonType, "Individual", "Pessoa fisica", 1),
        Option(ConfigurableOptionCategory.DonorPersonType, "Company", "Pessoa juridica", 2),
        Option(ConfigurableOptionCategory.DonorStatus, "Lead", "Lead", 1, "blue"),
        Option(ConfigurableOptionCategory.DonorStatus, "Active", "Ativo", 2, "green"),
        Option(ConfigurableOptionCategory.DonorStatus, "Inactive", "Inativo", 3, "neutral"),
        Option(ConfigurableOptionCategory.DonorStatus, "AtRisk", "Em risco", 4, "yellow"),
        Option(ConfigurableOptionCategory.DonorStatus, "DoNotContact", "Nao contatar", 5, "red"),
        Option(ConfigurableOptionCategory.RelationshipProfile, "New", "Novo", 1),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Recurring", "Recorrente", 2),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Major", "Grande doador", 3),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Lapsed", "Inativo", 4),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Reactivated", "Reativado", 5),
        Option(ConfigurableOptionCategory.RelationshipProfile, "Prospect", "Prospect", 6),
        Option(ConfigurableOptionCategory.DonorSource, "Manual", "Manual", 1),
        Option(ConfigurableOptionCategory.DonorSource, "Referral", "Indicacao", 2),
        Option(ConfigurableOptionCategory.DonorSource, "Phone", "Telefone", 3),
        Option(ConfigurableOptionCategory.DonorSource, "WhatsApp", "WhatsApp", 4),
        Option(ConfigurableOptionCategory.DonorSource, "Email", "E-mail", 5),
        Option(ConfigurableOptionCategory.DonorSource, "SocialMedia", "Redes sociais", 6),
        Option(ConfigurableOptionCategory.DonorSource, "Website", "Website", 7),
        Option(ConfigurableOptionCategory.DonorSource, "Event", "Evento", 8),
        Option(ConfigurableOptionCategory.DonorSource, "Import", "Importacao", 9),
        Option(ConfigurableOptionCategory.DonorSource, "Other", "Outro", 10),
        Option(ConfigurableOptionCategory.ContactChannel, "Phone", "Telefone", 1),
        Option(ConfigurableOptionCategory.ContactChannel, "WhatsApp", "WhatsApp", 2),
        Option(ConfigurableOptionCategory.ContactChannel, "Email", "E-mail", 3),
        Option(ConfigurableOptionCategory.ContactChannel, "Other", "Outro", 4),
        Option(ConfigurableOptionCategory.DonationType, "OneTime", "Pontual", 1),
        Option(ConfigurableOptionCategory.DonationType, "Recurring", "Recorrente", 2),
        Option(ConfigurableOptionCategory.DonationType, "Pledge", "Promessa", 3),
        Option(ConfigurableOptionCategory.DonationStatus, "Pending", "Pendente", 1, "yellow"),
        Option(ConfigurableOptionCategory.DonationStatus, "Confirmed", "Confirmada", 2, "green"),
        Option(ConfigurableOptionCategory.DonationStatus, "Overdue", "Vencida", 3, "red"),
        Option(ConfigurableOptionCategory.DonationStatus, "Cancelled", "Cancelada", 4),
        Option(ConfigurableOptionCategory.DonationStatus, "Refunded", "Estornada", 5, "yellow"),
        Option(ConfigurableOptionCategory.PaymentMethod, "Pix", "Pix", 1),
        Option(ConfigurableOptionCategory.PaymentMethod, "Boleto", "Boleto", 2),
        Option(ConfigurableOptionCategory.PaymentMethod, "CreditCard", "Cartao de credito", 3),
        Option(ConfigurableOptionCategory.PaymentMethod, "BankTransfer", "Transferencia bancaria", 4),
        Option(ConfigurableOptionCategory.PaymentMethod, "Cash", "Dinheiro", 5),
        Option(ConfigurableOptionCategory.PaymentMethod, "Other", "Outro", 6),
        Option(ConfigurableOptionCategory.TaskType, "Call", "Ligacao", 1),
        Option(ConfigurableOptionCategory.TaskType, "WhatsApp", "WhatsApp", 2),
        Option(ConfigurableOptionCategory.TaskType, "PaymentReminder", "Lembrete de pagamento", 3),
        Option(ConfigurableOptionCategory.TaskType, "Email", "E-mail", 4),
        Option(ConfigurableOptionCategory.TaskType, "FollowUp", "Follow-up", 5),
        Option(ConfigurableOptionCategory.TaskType, "ThankYou", "Agradecimento", 6),
        Option(ConfigurableOptionCategory.TaskType, "DataUpdate", "Atualizacao cadastral", 7),
        Option(ConfigurableOptionCategory.TaskType, "Other", "Outra", 8),
        Option(ConfigurableOptionCategory.TaskPriority, "Low", "Baixa", 1),
        Option(ConfigurableOptionCategory.TaskPriority, "Medium", "Media", 2),
        Option(ConfigurableOptionCategory.TaskPriority, "High", "Alta", 3),
        Option(ConfigurableOptionCategory.TaskPriority, "Urgent", "Urgente", 4),
        Option(ConfigurableOptionCategory.TaskStatus, "Open", "Aberta", 1, "blue"),
        Option(ConfigurableOptionCategory.TaskStatus, "InProgress", "Em andamento", 2, "yellow"),
        Option(ConfigurableOptionCategory.TaskStatus, "Completed", "Concluida", 3, "green"),
        Option(ConfigurableOptionCategory.TaskStatus, "Cancelled", "Cancelada", 4),
        Option(ConfigurableOptionCategory.ContactOutcome, "Reached", "Contato realizado", 1),
        Option(ConfigurableOptionCategory.ContactOutcome, "NoAnswer", "Nao atendeu", 2),
        Option(ConfigurableOptionCategory.ContactOutcome, "InvalidContact", "Contato invalido", 3),
        Option(ConfigurableOptionCategory.ContactOutcome, "RequestedCallback", "Retorno solicitado", 4),
        Option(ConfigurableOptionCategory.ContactOutcome, "DonationConfirmed", "Doacao confirmada", 5),
        Option(ConfigurableOptionCategory.ContactOutcome, "NotInterested", "Sem interesse", 6),
        Option(ConfigurableOptionCategory.ContactOutcome, "DoNotContact", "Nao contatar", 7),
        Option(ConfigurableOptionCategory.ContactOutcome, "Other", "Outro", 8),
        Option(ConfigurableOptionCategory.CampaignType, "Acquisition", "Captacao", 1),
        Option(ConfigurableOptionCategory.CampaignType, "Fundraising", "Captacao", 2),
        Option(ConfigurableOptionCategory.CampaignType, "Retention", "Retencao", 3),
        Option(ConfigurableOptionCategory.CampaignType, "Reactivation", "Reativacao", 4),
        Option(ConfigurableOptionCategory.CampaignType, "Emergency", "Emergencial", 5),
        Option(ConfigurableOptionCategory.CampaignType, "Other", "Outra", 6),
        Option(ConfigurableOptionCategory.CampaignStatus, "Draft", "Rascunho", 1),
        Option(ConfigurableOptionCategory.CampaignStatus, "Active", "Ativa", 2),
        Option(ConfigurableOptionCategory.CampaignStatus, "Completed", "Concluida", 3),
        Option(ConfigurableOptionCategory.CampaignStatus, "Cancelled", "Cancelada", 4),
        Option(ConfigurableOptionCategory.CampaignChannel, "Mixed", "Multicanal", 1),
        Option(ConfigurableOptionCategory.CampaignChannel, "Phone", "Telefone", 2),
        Option(ConfigurableOptionCategory.CampaignChannel, "WhatsApp", "WhatsApp", 3),
        Option(ConfigurableOptionCategory.CampaignChannel, "Email", "E-mail", 4),
        Option(ConfigurableOptionCategory.CampaignChannel, "SocialMedia", "Redes sociais", 5),
        Option(ConfigurableOptionCategory.CampaignChannel, "InPerson", "Presencial", 6),
        Option(ConfigurableOptionCategory.CampaignChannel, "Other", "Outro", 7),
        Option(ConfigurableOptionCategory.DonationPlanStatus, "Active", "Ativa", 1, "green"),
        Option(ConfigurableOptionCategory.DonationPlanStatus, "Paused", "Pausada", 2, "yellow"),
        Option(ConfigurableOptionCategory.DonationPlanStatus, "Cancelled", "Cancelada", 3),
        Option(ConfigurableOptionCategory.TimelineType, "Note", "Nota", 1),
        Option(ConfigurableOptionCategory.TimelineType, "Donation", "Contribuicao", 2, "green"),
        Option(ConfigurableOptionCategory.TimelineType, "Task", "Tarefa", 3, "blue"),
        Option(ConfigurableOptionCategory.TimelineType, "Contact", "Contato", 4, "yellow"),
        Option(ConfigurableOptionCategory.PhoneType, "Mobile", "Celular", 1),
        Option(ConfigurableOptionCategory.PhoneType, "WhatsApp", "WhatsApp", 2),
        Option(ConfigurableOptionCategory.PhoneType, "Home", "Residencial", 3),
        Option(ConfigurableOptionCategory.PhoneType, "Work", "Comercial", 4),
        Option(ConfigurableOptionCategory.EmailType, "Personal", "Pessoal", 1),
        Option(ConfigurableOptionCategory.EmailType, "Work", "Comercial", 2),
        Option(ConfigurableOptionCategory.EmailType, "Billing", "Cobranca", 3),
    ];

    private static IReadOnlyCollection<DefaultTag> DefaultTags() =>
    [
        new("Recorrente", "Doador com relacionamento recorrente."),
        new("Alto valor", "Doador com historico relevante de contribuicoes."),
    ];

    private static DefaultOption Option(ConfigurableOptionCategory category, string code, string name, int sortOrder, string? color = null) =>
        new(category, code, name, sortOrder, color);

    private sealed record DefaultOption(ConfigurableOptionCategory Category, string Code, string Name, int SortOrder, string? Color);

    private sealed record DefaultTag(string Name, string Description);
}
