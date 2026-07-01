using VinculoBackend.Domain.Constants;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Application.Common.Interfaces;
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
    private readonly IOrganizationDefaultsService _organizationDefaultsService;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationDefaultsService organizationDefaultsService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationDefaultsService = organizationDefaultsService;
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
        foreach (var roleName in new[] { Roles.SystemAdministrator, Roles.Administrator, Roles.Manager, Roles.Agent })
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedOrganizationAsync();
        await _organizationDefaultsService.EnsureDefaultsAsync(DemoOrganizationId, CancellationToken.None);

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
            await _userManager.AddToRolesAsync(administrator, [Roles.SystemAdministrator]);
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

        if (!await _userManager.IsInRoleAsync(administrator, Roles.SystemAdministrator))
        {
            await _userManager.AddToRoleAsync(administrator, Roles.SystemAdministrator);
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
            Name = "Administração",
            LegalName = "Administração",
            DefaultMonthlyGoal = 85000,
            TimeZone = "America/Sao_Paulo",
            Currency = "BRL",
            IsActive = true,
        });

        await _context.SaveChangesAsync();
    }

}
