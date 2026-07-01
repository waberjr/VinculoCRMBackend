using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VinculoBackend.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private static readonly Guid DemoOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PersonTypeIndividualId = Guid.Parse("11111111-1111-1111-1111-000000000001");
    private static readonly Guid PersonTypeCompanyId = Guid.Parse("11111111-1111-1111-1111-000000000002");
    private static readonly Guid DonorStatusLeadId = Guid.Parse("11111111-1111-1111-1111-000000000011");
    private static readonly Guid DonorStatusActiveId = Guid.Parse("11111111-1111-1111-1111-000000000012");
    private static readonly Guid DonorStatusInactiveId = Guid.Parse("11111111-1111-1111-1111-000000000013");
    private static readonly Guid DonorStatusAtRiskId = Guid.Parse("11111111-1111-1111-1111-000000000014");
    private static readonly Guid DonorStatusDoNotContactId = Guid.Parse("11111111-1111-1111-1111-000000000015");
    private static readonly Guid RelationshipNewId = Guid.Parse("11111111-1111-1111-1111-000000000021");
    private static readonly Guid RelationshipRecurringId = Guid.Parse("11111111-1111-1111-1111-000000000022");
    private static readonly Guid RelationshipMajorId = Guid.Parse("11111111-1111-1111-1111-000000000023");
    private static readonly Guid RelationshipLapsedId = Guid.Parse("11111111-1111-1111-1111-000000000024");
    private static readonly Guid RelationshipReactivatedId = Guid.Parse("11111111-1111-1111-1111-000000000025");
    private static readonly Guid RelationshipProspectId = Guid.Parse("11111111-1111-1111-1111-000000000026");
    private static readonly Guid SourceManualId = Guid.Parse("11111111-1111-1111-1111-000000000031");
    private static readonly Guid SourceReferralId = Guid.Parse("11111111-1111-1111-1111-000000000032");
    private static readonly Guid SourcePhoneId = Guid.Parse("11111111-1111-1111-1111-000000000033");
    private static readonly Guid SourceWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000034");
    private static readonly Guid SourceEmailId = Guid.Parse("11111111-1111-1111-1111-000000000035");
    private static readonly Guid SourceSocialMediaId = Guid.Parse("11111111-1111-1111-1111-000000000036");
    private static readonly Guid SourceWebsiteId = Guid.Parse("11111111-1111-1111-1111-000000000037");
    private static readonly Guid SourceEventId = Guid.Parse("11111111-1111-1111-1111-000000000038");
    private static readonly Guid SourceImportId = Guid.Parse("11111111-1111-1111-1111-000000000039");
    private static readonly Guid SourceOtherId = Guid.Parse("11111111-1111-1111-1111-000000000040");
    private static readonly Guid ChannelPhoneId = Guid.Parse("11111111-1111-1111-1111-000000000041");
    private static readonly Guid ChannelWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000042");
    private static readonly Guid ChannelEmailId = Guid.Parse("11111111-1111-1111-1111-000000000043");
    private static readonly Guid ChannelOtherId = Guid.Parse("11111111-1111-1111-1111-000000000044");
    private static readonly Guid DonationTypeOneTimeId = Guid.Parse("11111111-1111-1111-1111-000000000051");
    private static readonly Guid DonationTypeRecurringId = Guid.Parse("11111111-1111-1111-1111-000000000052");
    private static readonly Guid DonationTypePledgeId = Guid.Parse("11111111-1111-1111-1111-000000000053");
    private static readonly Guid DonationStatusPendingId = Guid.Parse("11111111-1111-1111-1111-000000000061");
    private static readonly Guid DonationStatusConfirmedId = Guid.Parse("11111111-1111-1111-1111-000000000062");
    private static readonly Guid DonationStatusOverdueId = Guid.Parse("11111111-1111-1111-1111-000000000063");
    private static readonly Guid DonationStatusCancelledId = Guid.Parse("11111111-1111-1111-1111-000000000064");
    private static readonly Guid DonationStatusRefundedId = Guid.Parse("11111111-1111-1111-1111-000000000065");
    private static readonly Guid PaymentPixId = Guid.Parse("11111111-1111-1111-1111-000000000071");
    private static readonly Guid PaymentBoletoId = Guid.Parse("11111111-1111-1111-1111-000000000072");
    private static readonly Guid PaymentCreditCardId = Guid.Parse("11111111-1111-1111-1111-000000000073");
    private static readonly Guid PaymentBankTransferId = Guid.Parse("11111111-1111-1111-1111-000000000074");
    private static readonly Guid PaymentCashId = Guid.Parse("11111111-1111-1111-1111-000000000075");
    private static readonly Guid PaymentOtherId = Guid.Parse("11111111-1111-1111-1111-000000000076");
    private static readonly Guid TaskTypeCallId = Guid.Parse("11111111-1111-1111-1111-000000000081");
    private static readonly Guid TaskTypeWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000082");
    private static readonly Guid TaskTypePaymentReminderId = Guid.Parse("11111111-1111-1111-1111-000000000083");
    private static readonly Guid TaskTypeEmailId = Guid.Parse("11111111-1111-1111-1111-000000000084");
    private static readonly Guid TaskTypeFollowUpId = Guid.Parse("11111111-1111-1111-1111-000000000085");
    private static readonly Guid TaskTypeThankYouId = Guid.Parse("11111111-1111-1111-1111-000000000086");
    private static readonly Guid TaskTypeDataUpdateId = Guid.Parse("11111111-1111-1111-1111-000000000087");
    private static readonly Guid TaskTypeOtherId = Guid.Parse("11111111-1111-1111-1111-000000000088");
    private static readonly Guid TaskPriorityLowId = Guid.Parse("11111111-1111-1111-1111-000000000091");
    private static readonly Guid TaskPriorityMediumId = Guid.Parse("11111111-1111-1111-1111-000000000092");
    private static readonly Guid TaskPriorityHighId = Guid.Parse("11111111-1111-1111-1111-000000000093");
    private static readonly Guid TaskPriorityUrgentId = Guid.Parse("11111111-1111-1111-1111-000000000094");
    private static readonly Guid TaskStatusOpenId = Guid.Parse("11111111-1111-1111-1111-000000000101");
    private static readonly Guid TaskStatusInProgressId = Guid.Parse("11111111-1111-1111-1111-000000000102");
    private static readonly Guid TaskStatusCompletedId = Guid.Parse("11111111-1111-1111-1111-000000000103");
    private static readonly Guid TaskStatusCancelledId = Guid.Parse("11111111-1111-1111-1111-000000000104");
    private static readonly Guid OutcomeReachedId = Guid.Parse("11111111-1111-1111-1111-000000000111");
    private static readonly Guid OutcomeNoAnswerId = Guid.Parse("11111111-1111-1111-1111-000000000112");
    private static readonly Guid OutcomeInvalidContactId = Guid.Parse("11111111-1111-1111-1111-000000000113");
    private static readonly Guid OutcomeRequestedCallbackId = Guid.Parse("11111111-1111-1111-1111-000000000114");
    private static readonly Guid OutcomeDonationConfirmedId = Guid.Parse("11111111-1111-1111-1111-000000000115");
    private static readonly Guid OutcomeNotInterestedId = Guid.Parse("11111111-1111-1111-1111-000000000116");
    private static readonly Guid OutcomeDoNotContactId = Guid.Parse("11111111-1111-1111-1111-000000000117");
    private static readonly Guid OutcomeOtherId = Guid.Parse("11111111-1111-1111-1111-000000000118");
    private static readonly Guid CampaignTypeAcquisitionId = Guid.Parse("11111111-1111-1111-1111-000000000121");
    private static readonly Guid CampaignTypeFundraisingId = Guid.Parse("11111111-1111-1111-1111-000000000122");
    private static readonly Guid CampaignTypeRetentionId = Guid.Parse("11111111-1111-1111-1111-000000000123");
    private static readonly Guid CampaignTypeReactivationId = Guid.Parse("11111111-1111-1111-1111-000000000124");
    private static readonly Guid CampaignTypeEmergencyId = Guid.Parse("11111111-1111-1111-1111-000000000125");
    private static readonly Guid CampaignTypeOtherId = Guid.Parse("11111111-1111-1111-1111-000000000126");
    private static readonly Guid CampaignStatusDraftId = Guid.Parse("11111111-1111-1111-1111-000000000130");
    private static readonly Guid CampaignStatusActiveId = Guid.Parse("11111111-1111-1111-1111-000000000131");
    private static readonly Guid CampaignStatusCompletedId = Guid.Parse("11111111-1111-1111-1111-000000000132");
    private static readonly Guid CampaignStatusCancelledId = Guid.Parse("11111111-1111-1111-1111-000000000133");
    private static readonly Guid CampaignChannelMixedId = Guid.Parse("11111111-1111-1111-1111-000000000141");
    private static readonly Guid CampaignChannelPhoneId = Guid.Parse("11111111-1111-1111-1111-000000000142");
    private static readonly Guid CampaignChannelWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000143");
    private static readonly Guid CampaignChannelEmailId = Guid.Parse("11111111-1111-1111-1111-000000000144");
    private static readonly Guid CampaignChannelSocialMediaId = Guid.Parse("11111111-1111-1111-1111-000000000145");
    private static readonly Guid CampaignChannelInPersonId = Guid.Parse("11111111-1111-1111-1111-000000000146");
    private static readonly Guid CampaignChannelOtherId = Guid.Parse("11111111-1111-1111-1111-000000000147");
    private static readonly Guid DonationPlanStatusActiveId = Guid.Parse("11111111-1111-1111-1111-000000000151");
    private static readonly Guid DonationPlanStatusPausedId = Guid.Parse("11111111-1111-1111-1111-000000000152");
    private static readonly Guid DonationPlanStatusCancelledId = Guid.Parse("11111111-1111-1111-1111-000000000153");
    private static readonly Guid TimelineTypeNoteId = Guid.Parse("11111111-1111-1111-1111-000000000161");
    private static readonly Guid TimelineTypeDonationId = Guid.Parse("11111111-1111-1111-1111-000000000162");
    private static readonly Guid TimelineTypeTaskId = Guid.Parse("11111111-1111-1111-1111-000000000163");
    private static readonly Guid TimelineTypeContactId = Guid.Parse("11111111-1111-1111-1111-000000000164");
    private static readonly Guid PhoneTypeMobileId = Guid.Parse("11111111-1111-1111-1111-000000000171");
    private static readonly Guid PhoneTypeWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000172");
    private static readonly Guid PhoneTypeHomeId = Guid.Parse("11111111-1111-1111-1111-000000000173");
    private static readonly Guid PhoneTypeWorkId = Guid.Parse("11111111-1111-1111-1111-000000000174");
    private static readonly Guid EmailTypePersonalId = Guid.Parse("11111111-1111-1111-1111-000000000175");
    private static readonly Guid EmailTypeWorkId = Guid.Parse("11111111-1111-1111-1111-000000000176");
    private static readonly Guid EmailTypeBillingId = Guid.Parse("11111111-1111-1111-1111-000000000177");
    private static readonly Guid TagRecurringId = Guid.Parse("11111111-1111-1111-1111-000000000201");
    private static readonly Guid TagMajorId = Guid.Parse("11111111-1111-1111-1111-000000000202");
    private static readonly Guid DonorAnaId = Guid.Parse("11111111-1111-1111-1111-000000001001");
    private static readonly Guid DonorCarlosId = Guid.Parse("11111111-1111-1111-1111-000000001002");
    private static readonly Guid DonorCompanyId = Guid.Parse("11111111-1111-1111-1111-000000001003");
    private static readonly Guid CampaignWinterId = Guid.Parse("11111111-1111-1111-1111-000000002001");

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        foreach (var roleName in new[] { Roles.Administrator, Roles.Manager, Roles.Agent })
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedOrganizationAsync();
        await SeedConfigurableOptionsAsync();
        await SeedDonorTagsAsync();
        await SeedCampaignsAsync();
        await SeedDonorsAsync();
        await SeedDonationsAsync();
        await SeedRelationshipTasksAsync();

        var administrator = await _userManager.FindByNameAsync("administrator@localhost");

        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                UserName = "administrator@localhost",
                Email = "administrator@localhost",
                DisplayName = "Administrator",
                OrganizationId = DemoOrganizationId,
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(administrator, "Administrator1!");
            await _userManager.AddToRolesAsync(administrator, new [] { Roles.Administrator });
        }
        else
        {
            administrator.DisplayName = string.IsNullOrWhiteSpace(administrator.DisplayName)
                ? "Administrator"
                : administrator.DisplayName;
            administrator.OrganizationId ??= DemoOrganizationId;
            administrator.EmailConfirmed = true;
            await _userManager.UpdateAsync(administrator);
        }

        if (!await _context.OrganizationMembers.AnyAsync(member => member.OrganizationId == DemoOrganizationId && member.UserId == administrator.Id))
        {
            _context.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = DemoOrganizationId,
                UserId = administrator.Id,
                Role = Roles.Administrator,
                IsActive = true,
                JoinedAtUtc = DateTimeOffset.UtcNow,
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedOrganizationAsync()
    {
        if (await _context.Organizations.AnyAsync(o => o.Id == DemoOrganizationId))
        {
            return;
        }

        _context.Organizations.Add(new Organization
        {
            Id = DemoOrganizationId,
            Name = "Instituto Esperanca Viva",
            LegalName = "Instituto Esperanca Viva",
            DefaultMonthlyGoal = 85000,
            TimeZone = "America/Sao_Paulo",
            Currency = "BRL",
            IsActive = true,
        });

        await _context.SaveChangesAsync();
    }

    private async Task SeedConfigurableOptionsAsync()
    {
        var options = new[]
        {
            Option(PersonTypeIndividualId, "DonorPersonType", "Individual", "Pessoa fisica", 1),
            Option(PersonTypeCompanyId, "DonorPersonType", "Company", "Pessoa juridica", 2),
            Option(DonorStatusLeadId, "DonorStatus", "Lead", "Lead", 1, "blue"),
            Option(DonorStatusActiveId, "DonorStatus", "Active", "Ativo", 2, "green"),
            Option(DonorStatusInactiveId, "DonorStatus", "Inactive", "Inativo", 3, "neutral"),
            Option(DonorStatusAtRiskId, "DonorStatus", "AtRisk", "Em risco", 4, "yellow"),
            Option(DonorStatusDoNotContactId, "DonorStatus", "DoNotContact", "Nao contatar", 5, "red"),
            Option(RelationshipNewId, "RelationshipProfile", "New", "Novo", 1),
            Option(RelationshipRecurringId, "RelationshipProfile", "Recurring", "Recorrente", 2),
            Option(RelationshipMajorId, "RelationshipProfile", "Major", "Grande doador", 3),
            Option(RelationshipLapsedId, "RelationshipProfile", "Lapsed", "Inativo", 4),
            Option(RelationshipReactivatedId, "RelationshipProfile", "Reactivated", "Reativado", 5),
            Option(RelationshipProspectId, "RelationshipProfile", "Prospect", "Prospect", 6),
            Option(SourceManualId, "DonorSource", "Manual", "Manual", 1),
            Option(SourceReferralId, "DonorSource", "Referral", "Indicacao", 2),
            Option(SourcePhoneId, "DonorSource", "Phone", "Telefone", 3),
            Option(SourceWhatsAppId, "DonorSource", "WhatsApp", "WhatsApp", 4),
            Option(SourceEmailId, "DonorSource", "Email", "E-mail", 5),
            Option(SourceSocialMediaId, "DonorSource", "SocialMedia", "Redes sociais", 6),
            Option(SourceWebsiteId, "DonorSource", "Website", "Website", 7),
            Option(SourceEventId, "DonorSource", "Event", "Evento", 8),
            Option(SourceImportId, "DonorSource", "Import", "Importacao", 9),
            Option(SourceOtherId, "DonorSource", "Other", "Outro", 10),
            Option(ChannelPhoneId, "ContactChannel", "Phone", "Telefone", 1),
            Option(ChannelWhatsAppId, "ContactChannel", "WhatsApp", "WhatsApp", 2),
            Option(ChannelEmailId, "ContactChannel", "Email", "E-mail", 3),
            Option(ChannelOtherId, "ContactChannel", "Other", "Outro", 4),
            Option(DonationTypeOneTimeId, "DonationType", "OneTime", "Pontual", 1),
            Option(DonationTypeRecurringId, "DonationType", "Recurring", "Recorrente", 2),
            Option(DonationTypePledgeId, "DonationType", "Pledge", "Promessa", 3),
            Option(DonationStatusPendingId, "DonationStatus", "Pending", "Pendente", 1, "yellow"),
            Option(DonationStatusConfirmedId, "DonationStatus", "Confirmed", "Confirmada", 2, "green"),
            Option(DonationStatusOverdueId, "DonationStatus", "Overdue", "Vencida", 3, "red"),
            Option(DonationStatusCancelledId, "DonationStatus", "Cancelled", "Cancelada", 4),
            Option(DonationStatusRefundedId, "DonationStatus", "Refunded", "Estornada", 5, "yellow"),
            Option(PaymentPixId, "PaymentMethod", "Pix", "Pix", 1),
            Option(PaymentBoletoId, "PaymentMethod", "Boleto", "Boleto", 2),
            Option(PaymentCreditCardId, "PaymentMethod", "CreditCard", "Cartao de credito", 3),
            Option(PaymentBankTransferId, "PaymentMethod", "BankTransfer", "Transferencia bancaria", 4),
            Option(PaymentCashId, "PaymentMethod", "Cash", "Dinheiro", 5),
            Option(PaymentOtherId, "PaymentMethod", "Other", "Outro", 6),
            Option(TaskTypeCallId, "TaskType", "Call", "Ligacao", 1),
            Option(TaskTypeWhatsAppId, "TaskType", "WhatsApp", "WhatsApp", 2),
            Option(TaskTypePaymentReminderId, "TaskType", "PaymentReminder", "Lembrete de pagamento", 3),
            Option(TaskTypeEmailId, "TaskType", "Email", "E-mail", 4),
            Option(TaskTypeFollowUpId, "TaskType", "FollowUp", "Follow-up", 5),
            Option(TaskTypeThankYouId, "TaskType", "ThankYou", "Agradecimento", 6),
            Option(TaskTypeDataUpdateId, "TaskType", "DataUpdate", "Atualizacao cadastral", 7),
            Option(TaskTypeOtherId, "TaskType", "Other", "Outra", 8),
            Option(TaskPriorityLowId, "TaskPriority", "Low", "Baixa", 1),
            Option(TaskPriorityMediumId, "TaskPriority", "Medium", "Media", 2),
            Option(TaskPriorityHighId, "TaskPriority", "High", "Alta", 3),
            Option(TaskPriorityUrgentId, "TaskPriority", "Urgent", "Urgente", 4),
            Option(TaskStatusOpenId, "TaskStatus", "Open", "Aberta", 1, "blue"),
            Option(TaskStatusInProgressId, "TaskStatus", "InProgress", "Em andamento", 2, "yellow"),
            Option(TaskStatusCompletedId, "TaskStatus", "Completed", "Concluida", 3, "green"),
            Option(TaskStatusCancelledId, "TaskStatus", "Cancelled", "Cancelada", 4),
            Option(OutcomeReachedId, "ContactOutcome", "Reached", "Contato realizado", 1),
            Option(OutcomeNoAnswerId, "ContactOutcome", "NoAnswer", "Nao atendeu", 2),
            Option(OutcomeInvalidContactId, "ContactOutcome", "InvalidContact", "Contato invalido", 3),
            Option(OutcomeRequestedCallbackId, "ContactOutcome", "RequestedCallback", "Retorno solicitado", 4),
            Option(OutcomeDonationConfirmedId, "ContactOutcome", "DonationConfirmed", "Doacao confirmada", 5),
            Option(OutcomeNotInterestedId, "ContactOutcome", "NotInterested", "Sem interesse", 6),
            Option(OutcomeDoNotContactId, "ContactOutcome", "DoNotContact", "Nao contatar", 7),
            Option(OutcomeOtherId, "ContactOutcome", "Other", "Outro", 8),
            Option(CampaignTypeAcquisitionId, "CampaignType", "Acquisition", "Captacao", 1),
            Option(CampaignTypeFundraisingId, "CampaignType", "Fundraising", "Captacao", 2),
            Option(CampaignTypeRetentionId, "CampaignType", "Retention", "Retencao", 3),
            Option(CampaignTypeReactivationId, "CampaignType", "Reactivation", "Reativacao", 4),
            Option(CampaignTypeEmergencyId, "CampaignType", "Emergency", "Emergencial", 5),
            Option(CampaignTypeOtherId, "CampaignType", "Other", "Outra", 6),
            Option(CampaignStatusDraftId, "CampaignStatus", "Draft", "Rascunho", 1),
            Option(CampaignStatusActiveId, "CampaignStatus", "Active", "Ativa", 2),
            Option(CampaignStatusCompletedId, "CampaignStatus", "Completed", "Concluida", 3),
            Option(CampaignStatusCancelledId, "CampaignStatus", "Cancelled", "Cancelada", 4),
            Option(CampaignChannelMixedId, "CampaignChannel", "Mixed", "Multicanal", 1),
            Option(CampaignChannelPhoneId, "CampaignChannel", "Phone", "Telefone", 2),
            Option(CampaignChannelWhatsAppId, "CampaignChannel", "WhatsApp", "WhatsApp", 3),
            Option(CampaignChannelEmailId, "CampaignChannel", "Email", "E-mail", 4),
            Option(CampaignChannelSocialMediaId, "CampaignChannel", "SocialMedia", "Redes sociais", 5),
            Option(CampaignChannelInPersonId, "CampaignChannel", "InPerson", "Presencial", 6),
            Option(CampaignChannelOtherId, "CampaignChannel", "Other", "Outro", 7),
            Option(DonationPlanStatusActiveId, "DonationPlanStatus", "Active", "Ativa", 1, "green"),
            Option(DonationPlanStatusPausedId, "DonationPlanStatus", "Paused", "Pausada", 2, "yellow"),
            Option(DonationPlanStatusCancelledId, "DonationPlanStatus", "Cancelled", "Cancelada", 3),
            Option(TimelineTypeNoteId, "TimelineType", "Note", "Nota", 1),
            Option(TimelineTypeDonationId, "TimelineType", "Donation", "Contribuicao", 2, "green"),
            Option(TimelineTypeTaskId, "TimelineType", "Task", "Tarefa", 3, "blue"),
            Option(TimelineTypeContactId, "TimelineType", "Contact", "Contato", 4, "yellow"),
            Option(PhoneTypeMobileId, "PhoneType", "Mobile", "Celular", 1),
            Option(PhoneTypeWhatsAppId, "PhoneType", "WhatsApp", "WhatsApp", 2),
            Option(PhoneTypeHomeId, "PhoneType", "Home", "Residencial", 3),
            Option(PhoneTypeWorkId, "PhoneType", "Work", "Comercial", 4),
            Option(EmailTypePersonalId, "EmailType", "Personal", "Pessoal", 1),
            Option(EmailTypeWorkId, "EmailType", "Work", "Comercial", 2),
            Option(EmailTypeBillingId, "EmailType", "Billing", "Cobranca", 3),
        };

        var existingIds = await _context.ConfigurableOptions
            .Where(option => option.OrganizationId == DemoOrganizationId)
            .Select(option => option.Id)
            .ToListAsync();

        foreach (var option in options.Where(option => !existingIds.Contains(option.Id)))
        {
            _context.ConfigurableOptions.Add(option);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDonorTagsAsync()
    {
        var existingIds = await _context.DonorTags
            .Where(tag => tag.OrganizationId == DemoOrganizationId)
            .Select(tag => tag.Id)
            .ToListAsync();

        if (!existingIds.Contains(TagRecurringId))
        {
            _context.DonorTags.Add(new DonorTag
            {
                Id = TagRecurringId,
                OrganizationId = DemoOrganizationId,
                Name = "Recorrente",
                Description = "Doador com relacionamento recorrente.",
            });
        }

        if (!existingIds.Contains(TagMajorId))
        {
            _context.DonorTags.Add(new DonorTag
            {
                Id = TagMajorId,
                OrganizationId = DemoOrganizationId,
                Name = "Alto valor",
                Description = "Doador com historico relevante de contribuicoes.",
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedCampaignsAsync()
    {
        if (await _context.Campaigns.AnyAsync(campaign => campaign.Id == CampaignWinterId))
        {
            return;
        }

        var campaign = new Campaign
        {
            Id = CampaignWinterId,
            OrganizationId = DemoOrganizationId,
            Name = "Campanha Inverno Solidario",
            Description = "Campanha inicial para validacao do MVP.",
            TypeOptionId = CampaignTypeAcquisitionId,
            StatusOptionId = CampaignStatusActiveId,
            ChannelOptionId = CampaignChannelMixedId,
            GoalAmount = 35000,
        };
        campaign.SetPeriod(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(30));

        _context.Campaigns.Add(campaign);

        await _context.SaveChangesAsync();
    }

    private async Task SeedDonorsAsync()
    {
        var existingIds = await _context.Donors
            .Where(donor => donor.OrganizationId == DemoOrganizationId)
            .Select(donor => donor.Id)
            .ToListAsync();

        if (!existingIds.Contains(DonorAnaId))
        {
            var donor = new Donor
            {
                Id = DonorAnaId,
                OrganizationId = DemoOrganizationId,
                FullName = "Ana Pereira",
                PersonTypeOptionId = PersonTypeIndividualId,
                Document = "12345678910",
                Email = "ana.pereira@example.org",
                Phone = "+55 11 99999-0001",
                WhatsApp = "+55 11 99999-0001",
                City = "Sao Paulo",
                State = "SP",
                StatusOptionId = DonorStatusActiveId,
                SourceOptionId = SourceReferralId,
                RelationshipProfileOptionId = RelationshipRecurringId,
                PreferredContactChannelOptionId = ChannelWhatsAppId,
                AcquisitionCampaignId = CampaignWinterId,
                AssignedUserId = "administrator@localhost",
                Notes = "Doadora recorrente seedada para desenvolvimento.",
            };
            donor.TagAssignments.Add(new DonorTagAssignment { OrganizationId = DemoOrganizationId, DonorTagId = TagRecurringId });
            _context.Donors.Add(donor);
        }

        if (!existingIds.Contains(DonorCarlosId))
        {
            var donor = new Donor
            {
                Id = DonorCarlosId,
                OrganizationId = DemoOrganizationId,
                FullName = "Carlos Nogueira",
                PersonTypeOptionId = PersonTypeIndividualId,
                Email = "carlos.nogueira@example.org",
                Phone = "+55 11 98888-0002",
                City = "Campinas",
                State = "SP",
                StatusOptionId = DonorStatusAtRiskId,
                SourceOptionId = SourceManualId,
                RelationshipProfileOptionId = RelationshipNewId,
                PreferredContactChannelOptionId = ChannelPhoneId,
                AssignedUserId = "administrator@localhost",
            };
            _context.Donors.Add(donor);
        }

        if (!existingIds.Contains(DonorCompanyId))
        {
            var donor = new Donor
            {
                Id = DonorCompanyId,
                OrganizationId = DemoOrganizationId,
                FullName = "Grupo Horizonte Ltda.",
                PersonTypeOptionId = PersonTypeCompanyId,
                Document = "12345678000190",
                Email = "contato@grupohorizonte.example",
                Phone = "+55 11 3777-0000",
                City = "Sao Paulo",
                State = "SP",
                StatusOptionId = DonorStatusActiveId,
                SourceOptionId = SourceManualId,
                RelationshipProfileOptionId = RelationshipMajorId,
                PreferredContactChannelOptionId = ChannelPhoneId,
                AcquisitionCampaignId = CampaignWinterId,
                AssignedUserId = "administrator@localhost",
            };
            donor.TagAssignments.Add(new DonorTagAssignment { OrganizationId = DemoOrganizationId, DonorTagId = TagMajorId });
            _context.Donors.Add(donor);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDonationsAsync()
    {
        if (await _context.Donations.AnyAsync(donation => donation.OrganizationId == DemoOrganizationId))
        {
            return;
        }

        var donations = new[]
        {
            Donation(DonorAnaId, 250, DonationStatusConfirmedId, PaymentPixId, DateTimeOffset.UtcNow.AddDays(-3), "PIX-ANA-001"),
            Donation(DonorCompanyId, 1800, DonationStatusConfirmedId, PaymentBoletoId, DateTimeOffset.UtcNow.AddDays(-2), "BOL-HORIZONTE-001"),
            Donation(DonorCarlosId, 95, DonationStatusPendingId, PaymentPixId, null, "PEND-CARLOS-001", DateTimeOffset.UtcNow.AddDays(5)),
        };

        foreach (var donation in donations)
        {
            _context.Donations.Add(donation);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedRelationshipTasksAsync()
    {
        if (await _context.RelationshipTasks.AnyAsync(task => task.OrganizationId == DemoOrganizationId))
        {
            return;
        }

        _context.RelationshipTasks.AddRange(
            new RelationshipTask
            {
                OrganizationId = DemoOrganizationId,
                DonorId = DonorCarlosId,
                Title = "Retomar contato com Carlos",
                Description = "Confirmar interesse e atualizar preferencia de contato.",
                AssignedUserId = "administrator@localhost",
                CreatedByUserId = "administrator@localhost",
                TypeOptionId = TaskTypeCallId,
                PriorityOptionId = TaskPriorityHighId,
                StatusOptionId = TaskStatusOpenId,
                DueAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            },
            new RelationshipTask
            {
                OrganizationId = DemoOrganizationId,
                DonorId = DonorAnaId,
                CampaignId = CampaignWinterId,
                Title = "Agradecer contribuicao",
                Description = "Enviar mensagem de agradecimento pela doacao recente.",
                AssignedUserId = "administrator@localhost",
                CreatedByUserId = "administrator@localhost",
                TypeOptionId = TaskTypeWhatsAppId,
                PriorityOptionId = TaskPriorityMediumId,
                StatusOptionId = TaskStatusOpenId,
                DueAtUtc = DateTimeOffset.UtcNow,
            });

        await _context.SaveChangesAsync();
    }

    private static ConfigurableOption Option(Guid id, string category, string code, string name, int sortOrder, string? color = null)
    {
        return new ConfigurableOption
        {
            Id = id,
            OrganizationId = DemoOrganizationId,
            Category = category,
            Code = code,
            Name = name,
            Color = color,
            SortOrder = sortOrder,
            IsSystem = true,
            IsActive = true,
        };
    }

    private static Donation Donation(
        Guid donorId,
        decimal amount,
        Guid statusOptionId,
        Guid paymentMethodOptionId,
        DateTimeOffset? paidAtUtc,
        string reference,
        DateTimeOffset? expectedAtUtc = null)
    {
        var donation = new Donation
        {
            OrganizationId = DemoOrganizationId,
            DonorId = donorId,
            CampaignId = CampaignWinterId,
            TypeOptionId = DonationTypeOneTimeId,
            StatusOptionId = statusOptionId,
            PaymentMethodOptionId = paymentMethodOptionId,
            ExpectedAtUtc = expectedAtUtc,
            PaidAtUtc = paidAtUtc,
            Reference = reference,
            CreatedByUserId = "administrator@localhost",
        };

        donation.SetAmount(amount);
        return donation;
    }
}
