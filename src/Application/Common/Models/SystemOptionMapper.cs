using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Common.Models;

public static class SystemOptionMapper
{
    public static TEnum Parse<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        var normalizedValue = ConfigurableOptionCode.FromName(value);
        foreach (var option in Enum.GetValues<TEnum>())
        {
            if (Code(option).Equals(normalizedValue, StringComparison.OrdinalIgnoreCase))
            {
                return option;
            }
        }

        throw new Common.Exceptions.ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(typeof(TEnum).Name, $"Valor invalido: {value}.")
            ]);
    }

    public static OptionDto ToOptionDto<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var metadata = Metadata(value);

        return new OptionDto
        {
            Id = Guid.Empty,
            Category = typeof(TEnum).Name,
            Code = Code(value),
            Name = metadata.Name,
            Color = metadata.Color,
            SortOrder = metadata.SortOrder,
            IsSystem = true,
            IsActive = true,
        };
    }

    public static IReadOnlyCollection<OptionDto> ToOptionDtos<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(ToOptionDto)
            .OrderBy(option => option.SortOrder)
            .ThenBy(option => option.Name)
            .ToList();

    public static string Code<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        ConfigurableOptionCode.FromName(value.ToString());

    public static string Name<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        Metadata(value).Name;

    private static OptionMetadata Metadata<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return value switch
        {
            DonorPersonType.Individual => new("Pessoa fisica", 1),
            DonorPersonType.Company => new("Pessoa juridica", 2),
            DonorStatus.Lead => new("Lead", 1, "blue"),
            DonorStatus.Active => new("Ativo", 2, "green"),
            DonorStatus.Inactive => new("Inativo", 3, "neutral"),
            DonorStatus.AtRisk => new("Em risco", 4, "yellow"),
            DonorStatus.DoNotContact => new("Nao contatar", 5, "red"),
            DonationType.OneTime => new("Pontual", 1),
            DonationType.Recurring => new("Recorrente", 2),
            DonationType.Pledge => new("Promessa", 3),
            DonationStatus.Pending => new("Pendente", 1, "yellow"),
            DonationStatus.Confirmed => new("Confirmada", 2, "green"),
            DonationStatus.Overdue => new("Vencida", 3, "red"),
            DonationStatus.Cancelled => new("Cancelada", 4),
            DonationStatus.Refunded => new("Estornada", 5, "yellow"),
            PaymentMethod.Pix => new("Pix", 1),
            PaymentMethod.Boleto => new("Boleto", 2),
            PaymentMethod.CreditCard => new("Cartao de credito", 3),
            PaymentMethod.BankTransfer => new("Transferencia bancaria", 4),
            PaymentMethod.Cash => new("Dinheiro", 5),
            PaymentMethod.Other => new("Outro", 6),
            DonationPlanStatus.Active => new("Ativa", 1, "green"),
            DonationPlanStatus.Paused => new("Pausada", 2, "yellow"),
            DonationPlanStatus.Cancelled => new("Cancelada", 3),
            CampaignType.Fundraising => new("Captacao", 1),
            CampaignType.Retention => new("Retencao", 2),
            CampaignType.Reactivation => new("Reativacao", 3),
            CampaignType.Emergency => new("Emergencial", 4),
            CampaignType.Other => new("Outra", 5),
            CampaignStatus.Draft => new("Rascunho", 1),
            CampaignStatus.Active => new("Ativa", 2),
            CampaignStatus.Completed => new("Concluida", 3),
            CampaignStatus.Cancelled => new("Cancelada", 4),
            CampaignChannel.Phone => new("Telefone", 1),
            CampaignChannel.WhatsApp => new("WhatsApp", 2),
            CampaignChannel.Email => new("E-mail", 3),
            CampaignChannel.SocialMedia => new("Redes sociais", 4),
            CampaignChannel.InPerson => new("Presencial", 5),
            CampaignChannel.Other => new("Outro", 6),
            TaskType.Call => new("Ligacao", 1),
            TaskType.WhatsApp => new("WhatsApp", 2),
            TaskType.Email => new("E-mail", 3),
            TaskType.FollowUp => new("Follow-up", 4),
            TaskType.PaymentReminder => new("Lembrete de pagamento", 5),
            TaskType.ThankYou => new("Agradecimento", 6),
            TaskType.DataUpdate => new("Atualizacao cadastral", 7),
            TaskType.Other => new("Outra", 8),
            TaskPriority.Low => new("Baixa", 1),
            TaskPriority.Medium => new("Media", 2),
            TaskPriority.High => new("Alta", 3),
            TaskPriority.Urgent => new("Urgente", 4),
            RelationshipTaskStatus.Open => new("Aberta", 1, "blue"),
            RelationshipTaskStatus.InProgress => new("Em andamento", 2, "yellow"),
            RelationshipTaskStatus.Completed => new("Concluida", 3, "green"),
            RelationshipTaskStatus.Cancelled => new("Cancelada", 4),
            ContactOutcome.Reached => new("Contato realizado", 1),
            ContactOutcome.NoAnswer => new("Nao atendeu", 2),
            ContactOutcome.InvalidContact => new("Contato invalido", 3),
            ContactOutcome.RequestedCallback => new("Retorno solicitado", 4),
            ContactOutcome.DonationConfirmed => new("Doacao confirmada", 5),
            ContactOutcome.NotInterested => new("Sem interesse", 6),
            ContactOutcome.DoNotContact => new("Nao contatar", 7),
            ContactOutcome.Other => new("Outro", 8),
            TimelineEntryType.Note => new("Nota", 1),
            TimelineEntryType.Donation => new("Contribuicao", 2, "green"),
            TimelineEntryType.Task => new("Tarefa", 3, "blue"),
            TimelineEntryType.Contact => new("Contato", 4, "yellow"),
            PhoneType.Mobile => new("Celular", 1),
            PhoneType.WhatsApp => new("WhatsApp", 2),
            PhoneType.Home => new("Residencial", 3),
            PhoneType.Work => new("Comercial", 4),
            EmailType.Personal => new("Pessoal", 1),
            EmailType.Work => new("Comercial", 2),
            EmailType.Billing => new("Cobranca", 3),
            _ => new(value.ToString(), 99),
        };
    }

    private sealed record OptionMetadata(string Name, int SortOrder, string? Color = null);
}
