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
    private static readonly Guid SourceManualId = Guid.Parse("11111111-1111-1111-1111-000000000031");
    private static readonly Guid SourceReferralId = Guid.Parse("11111111-1111-1111-1111-000000000032");
    private static readonly Guid ChannelPhoneId = Guid.Parse("11111111-1111-1111-1111-000000000041");
    private static readonly Guid ChannelWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000042");
    private static readonly Guid DonationTypeOneTimeId = Guid.Parse("11111111-1111-1111-1111-000000000051");
    private static readonly Guid DonationTypeRecurringId = Guid.Parse("11111111-1111-1111-1111-000000000052");
    private static readonly Guid DonationStatusPendingId = Guid.Parse("11111111-1111-1111-1111-000000000061");
    private static readonly Guid DonationStatusConfirmedId = Guid.Parse("11111111-1111-1111-1111-000000000062");
    private static readonly Guid DonationStatusOverdueId = Guid.Parse("11111111-1111-1111-1111-000000000063");
    private static readonly Guid PaymentPixId = Guid.Parse("11111111-1111-1111-1111-000000000071");
    private static readonly Guid PaymentBoletoId = Guid.Parse("11111111-1111-1111-1111-000000000072");
    private static readonly Guid PaymentCreditCardId = Guid.Parse("11111111-1111-1111-1111-000000000073");
    private static readonly Guid TaskTypeCallId = Guid.Parse("11111111-1111-1111-1111-000000000081");
    private static readonly Guid TaskTypeWhatsAppId = Guid.Parse("11111111-1111-1111-1111-000000000082");
    private static readonly Guid TaskTypePaymentReminderId = Guid.Parse("11111111-1111-1111-1111-000000000083");
    private static readonly Guid TaskPriorityLowId = Guid.Parse("11111111-1111-1111-1111-000000000091");
    private static readonly Guid TaskPriorityMediumId = Guid.Parse("11111111-1111-1111-1111-000000000092");
    private static readonly Guid TaskPriorityHighId = Guid.Parse("11111111-1111-1111-1111-000000000093");
    private static readonly Guid TaskPriorityUrgentId = Guid.Parse("11111111-1111-1111-1111-000000000094");
    private static readonly Guid TaskStatusOpenId = Guid.Parse("11111111-1111-1111-1111-000000000101");
    private static readonly Guid TaskStatusInProgressId = Guid.Parse("11111111-1111-1111-1111-000000000102");
    private static readonly Guid TaskStatusCompletedId = Guid.Parse("11111111-1111-1111-1111-000000000103");
    private static readonly Guid OutcomeReachedId = Guid.Parse("11111111-1111-1111-1111-000000000111");
    private static readonly Guid OutcomeNoAnswerId = Guid.Parse("11111111-1111-1111-1111-000000000112");
    private static readonly Guid CampaignTypeAcquisitionId = Guid.Parse("11111111-1111-1111-1111-000000000121");
    private static readonly Guid CampaignStatusActiveId = Guid.Parse("11111111-1111-1111-1111-000000000131");
    private static readonly Guid CampaignChannelMixedId = Guid.Parse("11111111-1111-1111-1111-000000000141");
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
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
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
                DisplayName = "Marina Alves",
                OrganizationId = DemoOrganizationId,
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }
        else
        {
            administrator.DisplayName = string.IsNullOrWhiteSpace(administrator.DisplayName)
                ? "Marina Alves"
                : administrator.DisplayName;
            administrator.OrganizationId ??= DemoOrganizationId;
            administrator.EmailConfirmed = true;
            await _userManager.UpdateAsync(administrator);
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
            Option(SourceManualId, "DonorSource", "Manual", "Manual", 1),
            Option(SourceReferralId, "DonorSource", "Referral", "Indicacao", 2),
            Option(ChannelPhoneId, "ContactChannel", "Phone", "Telefone", 1),
            Option(ChannelWhatsAppId, "ContactChannel", "WhatsApp", "WhatsApp", 2),
            Option(DonationTypeOneTimeId, "DonationType", "OneTime", "Pontual", 1),
            Option(DonationTypeRecurringId, "DonationType", "Recurring", "Recorrente", 2),
            Option(DonationStatusPendingId, "DonationStatus", "Pending", "Pendente", 1, "yellow"),
            Option(DonationStatusConfirmedId, "DonationStatus", "Confirmed", "Confirmada", 2, "green"),
            Option(DonationStatusOverdueId, "DonationStatus", "Overdue", "Vencida", 3, "red"),
            Option(PaymentPixId, "PaymentMethod", "Pix", "Pix", 1),
            Option(PaymentBoletoId, "PaymentMethod", "Boleto", "Boleto", 2),
            Option(PaymentCreditCardId, "PaymentMethod", "CreditCard", "Cartao de credito", 3),
            Option(TaskTypeCallId, "TaskType", "Call", "Ligacao", 1),
            Option(TaskTypeWhatsAppId, "TaskType", "WhatsApp", "WhatsApp", 2),
            Option(TaskTypePaymentReminderId, "TaskType", "PaymentReminder", "Lembrete de pagamento", 3),
            Option(TaskPriorityLowId, "TaskPriority", "Low", "Baixa", 1),
            Option(TaskPriorityMediumId, "TaskPriority", "Medium", "Media", 2),
            Option(TaskPriorityHighId, "TaskPriority", "High", "Alta", 3),
            Option(TaskPriorityUrgentId, "TaskPriority", "Urgent", "Urgente", 4),
            Option(TaskStatusOpenId, "TaskStatus", "Open", "Aberta", 1, "blue"),
            Option(TaskStatusInProgressId, "TaskStatus", "InProgress", "Em andamento", 2, "yellow"),
            Option(TaskStatusCompletedId, "TaskStatus", "Completed", "Concluida", 3, "green"),
            Option(OutcomeReachedId, "ContactOutcome", "Reached", "Contato realizado", 1),
            Option(OutcomeNoAnswerId, "ContactOutcome", "NoAnswer", "Nao atendeu", 2),
            Option(CampaignTypeAcquisitionId, "CampaignType", "Acquisition", "Captacao", 1),
            Option(CampaignStatusActiveId, "CampaignStatus", "Active", "Ativa", 1),
            Option(CampaignChannelMixedId, "CampaignChannel", "Mixed", "Multicanal", 1),
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

        _context.Campaigns.Add(new Campaign
        {
            Id = CampaignWinterId,
            OrganizationId = DemoOrganizationId,
            Name = "Campanha Inverno Solidario",
            Description = "Campanha inicial para validacao do MVP.",
            TypeOptionId = CampaignTypeAcquisitionId,
            StatusOptionId = CampaignStatusActiveId,
            ChannelOptionId = CampaignChannelMixedId,
            GoalAmount = 35000,
            StartDateUtc = DateTimeOffset.UtcNow.AddDays(-30),
            EndDateUtc = DateTimeOffset.UtcNow.AddDays(30),
        });

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
            _context.Donors.Add(new Donor
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
            });
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
