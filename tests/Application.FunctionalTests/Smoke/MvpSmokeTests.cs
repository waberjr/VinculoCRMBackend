using System;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using VinculoBackend.Application.DocumentAttachments.Commands.CreateDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.DocumentAttachments.Queries.CreateDocumentAttachmentAccessUrl;
using VinculoBackend.Application.DocumentAttachments.Queries.DownloadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;
using VinculoBackend.Application.Common.Exceptions;
using System.Threading.Tasks;
using VinculoBackend.Application.Campaigns.Commands.CreateCampaign;
using VinculoBackend.Application.Communications.Commands.CreateCommunicationCampaign;
using VinculoBackend.Application.Communications.Queries.GetCommunicationCampaignRecipients;
using VinculoBackend.Application.Communications.Queries.GetCommunicationCampaigns;
using VinculoBackend.Application.Dashboard.Queries.GetDashboardOverview;
using VinculoBackend.Application.Donations.Commands.CancelDonation;
using VinculoBackend.Application.Donations.Commands.ConfirmDonation;
using VinculoBackend.Application.Donations.Commands.CreateDonation;
using VinculoBackend.Application.Donations.Commands.RefundDonation;
using VinculoBackend.Application.Donors.Commands.CreateDonor;
using VinculoBackend.Application.Donors.Commands.UpdateDonor;
using VinculoBackend.Application.Donors.Queries.FindDonorDuplicates;
using VinculoBackend.Application.Donors.Queries.GetDonorById;
using VinculoBackend.Application.Donors.Queries.GetDonorTimeline;
using VinculoBackend.Application.Donors.Queries.GetDonorOperationalSegments;
using VinculoBackend.Application.Donors.Queries.GetDonors;
using VinculoBackend.Application.ImpactProjects.Commands.CreateProject;
using VinculoBackend.Application.ImpactProjects.Commands.UpdateProject;
using VinculoBackend.Application.ImpactProjects.Queries.ExportProjectAccountability;
using VinculoBackend.Application.ImpactProjects.Queries.GetProjectAccountability;
using VinculoBackend.Application.ImpactProjects.Queries.GetProjects;
using VinculoBackend.Application.Organizations.Commands.CreateOrganization;
using VinculoBackend.Application.Organizations.Models;
using VinculoBackend.Application.Receipts.Commands.IssueReceipt;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPdf;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;
using VinculoBackend.Application.RelationshipTasks.Commands.CompleteRelationshipTask;
using VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;
using VinculoBackend.Application.RelationshipTasks.Queries.GetRelationshipTasks;
using VinculoBackend.Application.Users.Commands.LoginUser;
using VinculoBackend.Domain.Constants;

namespace VinculoBackend.Application.FunctionalTests.Smoke;

public class MvpSmokeTests : TestBase
{
    [Test]
    public async Task ShouldLoginWithCreatedUser()
    {
        var password = "Testing1234!";
        await TestApp.RunAsUserAsync("login-smoke@local", password, []);

        var principal = await TestApp.SendAsync(new LoginUserCommand("login-smoke@local", password));

        principal.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldCreateDonorConfirmDonationAndShowOnDashboard()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Maria Smoke");
        var donationId = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            Amount = 150,
            Type = "OneTime",
            Status = "Pending",
            PaymentMethod = "Pix",
            ExpectedAtUtc = DateTimeOffset.UtcNow.AddDays(2),
        });

        var paidAt = DateTimeOffset.UtcNow;
        await TestApp.SendAsync(new ConfirmDonationCommand(donationId, paidAt, "SMOKE-001"));

        var dashboard = await TestApp.SendAsync(new GetDashboardOverviewQuery
        {
            StartDateUtc = paidAt.AddDays(-1),
            EndDateUtc = paidAt.AddDays(1),
        });

        dashboard.LatestDonations.ShouldContain(item => item.Id == donationId && item.Amount == 150);
        dashboard.DonationsByDay.Sum(item => item.Amount).ShouldBe(150);
    }

    [Test]
    public async Task ShouldCancelDonationAndRegisterTimeline()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Cancelamento");
        var donationId = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            Amount = 80,
            Type = "OneTime",
            Status = "Pending",
            PaymentMethod = "Pix",
            ExpectedAtUtc = DateTimeOffset.UtcNow.AddDays(3),
        });

        await TestApp.SendAsync(new CancelDonationCommand(donationId, "Doador desistiu da contribuicao."));

        var timeline = await TestApp.SendAsync(new GetDonorTimelineQuery(donorId));

        timeline.ShouldNotBeNull();
        timeline.Items.ShouldContain(entry =>
            entry.Title == "Contribuicao cancelada" &&
            entry.Description.Contains("Doador desistiu da contribuicao.", StringComparison.Ordinal));
    }

    [Test]
    public async Task ShouldRefundDonationAndRegisterTimeline()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Estorno");
        var donationId = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            Amount = 95,
            Type = "OneTime",
            Status = "Confirmed",
            PaymentMethod = "Pix",
            PaidAtUtc = DateTimeOffset.UtcNow,
            Reference = "REFUND-SMOKE-001",
        });

        await TestApp.SendAsync(new RefundDonationCommand(donationId, "Pagamento duplicado."));

        var timeline = await TestApp.SendAsync(new GetDonorTimelineQuery(donorId));

        timeline.ShouldNotBeNull();
        timeline.Items.ShouldContain(entry =>
            entry.Title == "Contribuicao estornada" &&
            entry.Description.Contains("Pagamento duplicado.", StringComparison.Ordinal));
    }

    [Test]
    public async Task ShouldCompleteTaskWithRequestedCallbackAndCreateFollowUp()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Carlos Follow");
        var originalTaskId = await TestApp.SendAsync(new CreateRelationshipTaskCommand
        {
            DonorId = donorId,
            Title = "Ligar para retorno",
            Type = "Call",
            Priority = "Medium",
            DueAtUtc = DateTimeOffset.UtcNow.AddHours(2),
        });

        var followUpAt = DateTimeOffset.UtcNow.AddDays(3);
        await TestApp.SendAsync(new CompleteRelationshipTaskCommand(
            originalTaskId,
            "RequestedCallback",
            "Solicitou retorno na proxima semana.",
            followUpAt,
            null));

        var tasks = await TestApp.SendAsync(new GetRelationshipTasksQuery
        {
            DonorId = donorId,
            PageSize = 10,
        });

        tasks.Items.ShouldContain(task => task.Id == originalTaskId && task.Status.Code == "completed");
        var followUp = tasks.Items.SingleOrDefault(task =>
            task.Id != originalTaskId &&
            task.Status.Code == "open" &&
            task.Type.Code == "follow-up");

        followUp.ShouldNotBeNull();
        followUp.DueAtUtc.ShouldNotBeNull();
        Math.Abs((followUp.DueAtUtc.Value - followUpAt).TotalSeconds).ShouldBeLessThan(1);
    }

    [Test]
    public async Task ShouldNotReadDonorFromAnotherOrganization()
    {
        var organizationA = await CreateAndUseOrganizationAsync("Organizacao A");
        var donorId = await CreateDonorAsync("Doador Organizacao A");
        await CreateAndUseOrganizationAsync("Organizacao B");

        var donorFromOtherOrganization = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));

        donorFromOtherOrganization.ShouldBeNull();
        TestApp.SetOrganizationId(organizationA);
        var donorFromOriginalOrganization = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));
        donorFromOriginalOrganization.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldFindPossibleDonorDuplicatesByEmailAndPhone()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Duplicado Smoke",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Email = "duplicado@example.com",
            Phone = "(62) 99999-0000",
            AllowsCommunication = true,
        });

        var duplicates = await TestApp.SendAsync(new FindDonorDuplicatesQuery
        {
            Email = "DUPLICADO@example.com",
            Phone = "62999990000",
        });

        duplicates.ShouldContain(duplicate =>
            duplicate.Id == donorId &&
            duplicate.MatchedFields.Contains("email") &&
            duplicate.MatchedFields.Contains("phone"));
    }

    [Test]
    public async Task ShouldCalculateOperationalDonorSegments()
    {
        await CreateAndUseOrganizationAsync();
        var overdueDonorId = await CreateDonorAsync("Doador Cobranca Vencida");
        var leadWithoutDonationId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Lead Sem Conversao",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Email = "lead-sem-conversao@example.com",
            AllowsCommunication = true,
        });

        await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = overdueDonorId,
            Amount = 90,
            Type = "OneTime",
            Status = "Pending",
            PaymentMethod = "Pix",
            ExpectedAtUtc = DateTimeOffset.UtcNow.AddDays(-2),
        });

        var segments = await TestApp.SendAsync(new GetDonorOperationalSegmentsQuery());
        var overdueSegment = segments.Single(segment => segment.Code == "OverdueDonations");
        var leadsSegment = segments.Single(segment => segment.Code == "LeadsWithoutDonation");
        var filteredDonors = await TestApp.SendAsync(new GetDonorsQuery
        {
            Segment = "OverdueDonations",
            PageSize = 10,
        });

        overdueSegment.Count.ShouldBeGreaterThanOrEqualTo(1);
        leadsSegment.Count.ShouldBeGreaterThanOrEqualTo(1);
        filteredDonors.Items.ShouldContain(donor => donor.Id == overdueDonorId);
        filteredDonors.Items.ShouldNotContain(donor => donor.Id == leadWithoutDonationId);
    }

    [Test]
    public async Task ShouldCreateDoNotContactDonorConsistently()
    {
        await CreateAndUseOrganizationAsync();

        var donorId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Doador Bloqueado",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Email = "bloqueado@example.com",
            AllowsCommunication = true,
            DoNotContact = true,
            DoNotContactReason = "Solicitou descadastro.",
        });

        var donor = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));

        donor.ShouldNotBeNull();
        donor.Status.Code.ShouldBe("do-not-contact");
        donor.AllowsCommunication.ShouldBeFalse();
        donor.DoNotContact.ShouldBeTrue();
        donor.DoNotContactReason.ShouldBe("Solicitou descadastro.");
    }

    [Test]
    public async Task ShouldNormalizeDonorContactsAndKeepSinglePrimary()
    {
        await CreateAndUseOrganizationAsync();

        var donorId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Doador Contatos",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Phones =
            [
                new VinculoBackend.Application.Donors.Commands.CreateDonor.DonorPhoneRequest("Mobile", "  62999990000  ", true),
                new VinculoBackend.Application.Donors.Commands.CreateDonor.DonorPhoneRequest("WhatsApp", "  62988880000  ", true),
            ],
            Emails =
            [
                new VinculoBackend.Application.Donors.Commands.CreateDonor.DonorEmailRequest("Personal", " pessoal@example.com ", false),
                new VinculoBackend.Application.Donors.Commands.CreateDonor.DonorEmailRequest("Work", " trabalho@example.com ", false),
            ],
        });

        var donor = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));

        donor.ShouldNotBeNull();
        donor.Phone.ShouldBe("62999990000");
        donor.WhatsApp.ShouldBe("62988880000");
        donor.Email.ShouldBe("pessoal@example.com");
        donor.Phones.Count(phone => phone.IsPrimary).ShouldBe(1);
        donor.Emails.Count(email => email.IsPrimary).ShouldBe(1);
    }

    [Test]
    public async Task ShouldUpdateDonorContactsAndKeepSinglePrimary()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Doador Atualizar Contatos",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Phone = "62911110000",
            Email = "antigo@example.com",
            AllowsCommunication = true,
            Tags = ["antiga", "recorrente"],
        });

        await TestApp.SendAsync(new UpdateDonorCommand
        {
            Id = donorId,
            FullName = "Doador Atualizar Contatos",
            PersonType = "Individual",
            Status = "Active",
            Source = "Manual",
            AllowsCommunication = true,
            Phones =
            [
                new VinculoBackend.Application.Donors.Commands.UpdateDonor.DonorPhoneRequest("Mobile", "  62922220000  ", false),
                new VinculoBackend.Application.Donors.Commands.UpdateDonor.DonorPhoneRequest("WhatsApp", "  62933330000  ", true),
                new VinculoBackend.Application.Donors.Commands.UpdateDonor.DonorPhoneRequest("Work", "  6233334444  ", true),
            ],
            Emails =
            [
                new VinculoBackend.Application.Donors.Commands.UpdateDonor.DonorEmailRequest("Personal", " novo@example.com ", false),
                new VinculoBackend.Application.Donors.Commands.UpdateDonor.DonorEmailRequest("Work", " trabalho@example.com ", true),
            ],
            Tags = ["nova", "recorrente"],
        });

        var donor = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));

        donor.ShouldNotBeNull();
        donor.Phone.ShouldBe("62933330000");
        donor.WhatsApp.ShouldBe("62933330000");
        donor.Email.ShouldBe("trabalho@example.com");
        donor.Phones.Count(phone => phone.IsPrimary).ShouldBe(1);
        donor.Phones.ShouldNotContain(phone => phone.Number == "62911110000");
        donor.Emails.Count(email => email.IsPrimary).ShouldBe(1);
        donor.Emails.ShouldNotContain(email => email.Address == "antigo@example.com");
        donor.Tags.Select(tag => tag.Name.ToLowerInvariant()).ShouldBe(["nova", "recorrente"], ignoreOrder: true);
        donor.Tags.ShouldNotContain(tag => tag.Name.Equals("antiga", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task ShouldUpdateDonorConsentAndRegisterTimeline()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Consentimento");

        await TestApp.SendAsync(new UpdateDonorCommand
        {
            Id = donorId,
            FullName = "Doador Consentimento",
            PersonType = "Individual",
            Status = "Active",
            Source = "Manual",
            AllowsCommunication = true,
            DoNotContact = true,
            DoNotContactReason = "Nao deseja mais contato.",
        });

        var donor = await TestApp.SendAsync(new GetDonorByIdQuery(donorId));
        var timeline = await TestApp.SendAsync(new GetDonorTimelineQuery(donorId));

        donor.ShouldNotBeNull();
        donor.Status.Code.ShouldBe("do-not-contact");
        donor.AllowsCommunication.ShouldBeFalse();
        donor.DoNotContactReason.ShouldBe("Nao deseja mais contato.");
        timeline.ShouldNotBeNull();
        timeline.Items.ShouldContain(entry => entry.Description == "Nao deseja mais contato.");
    }

    [Test]
    public async Task ShouldPlanCommunicationRespectingConsentAndTimeline()
    {
        await CreateAndUseOrganizationAsync();
        var allowedDonorId = await CreateDonorAsync("Doador Comunicavel");
        var blockedDonorId = await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = "Doador Nao Contatar",
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            AllowsCommunication = true,
            DoNotContact = true,
            DoNotContactReason = "Nao quer comunicacoes.",
        });

        var campaignId = await TestApp.SendAsync(new CreateCommunicationCampaignCommand(
            "Boletim interno",
            "Email",
            "Doadores selecionados",
            null,
            null,
            [allowedDonorId, blockedDonorId]));

        var campaigns = await TestApp.SendAsync(new GetCommunicationCampaignsQuery());
        var recipients = await TestApp.SendAsync(new GetCommunicationCampaignRecipientsQuery(campaignId));
        var allowedTimeline = await TestApp.SendAsync(new GetDonorTimelineQuery(allowedDonorId));
        var blockedTimeline = await TestApp.SendAsync(new GetDonorTimelineQuery(blockedDonorId));

        var campaign = campaigns.Single(item => item.Id == campaignId);
        campaign.RecipientsCount.ShouldBe(1);
        campaign.BlockedByConsentCount.ShouldBe(1);
        recipients.ShouldNotBeNull();
        recipients.ShouldContain(recipient => recipient.DonorId == allowedDonorId && recipient.Status == "Planned" && recipient.TimelineEntryId != null);
        recipients.ShouldContain(recipient => recipient.DonorId == blockedDonorId && recipient.Status == "Blocked" && recipient.TimelineEntryId == null);
        allowedTimeline.ShouldNotBeNull();
        allowedTimeline.Items.ShouldContain(entry =>
            entry.Title == "Comunicacao planejada: Boletim interno" &&
            entry.Description.Contains("Sem envio real.", StringComparison.Ordinal));
        blockedTimeline.ShouldNotBeNull();
        blockedTimeline.Items.ShouldNotContain(entry => entry.Title == "Comunicacao planejada: Boletim interno");
    }

    [Test]
    public async Task ShouldCreateAndListProjects()
    {
        await CreateAndUseOrganizationAsync();

        var projectId = await TestApp.SendAsync(new CreateProjectCommand
        {
            Name = " Projeto Smoke ",
            Description = "Prestacao de contas do projeto",
            GoalAmount = 2500,
            ImpactMetric = "50 familias atendidas",
            Status = "Active",
            StartDateUtc = DateTimeOffset.UtcNow.AddDays(-1),
            EndDateUtc = DateTimeOffset.UtcNow.AddDays(30),
        });

        var projects = await TestApp.SendAsync(new GetProjectsQuery
        {
            Search = "smoke",
            PageSize = 10,
        });

        projects.Items.ShouldContain(project =>
            project.Id == projectId &&
            project.Name == "Projeto Smoke" &&
            project.Status == "Active" &&
            project.GoalAmount == 2500);
    }

    [Test]
    public async Task ShouldUpdateProjectCampaignLinksWithoutDuplicatingArchivedLinks()
    {
        await CreateAndUseOrganizationAsync();
        var firstCampaignId = await TestApp.SendAsync(new CreateCampaignCommand
        {
            Name = "Campanha Projeto A",
            Type = "Fundraising",
            Channel = "Email",
            GoalAmount = 1000,
            StartDateUtc = DateTimeOffset.UtcNow.AddDays(-1),
            EndDateUtc = DateTimeOffset.UtcNow.AddDays(30),
        });
        var secondCampaignId = await TestApp.SendAsync(new CreateCampaignCommand
        {
            Name = "Campanha Projeto B",
            Type = "Fundraising",
            Channel = "Email",
            GoalAmount = 1000,
            StartDateUtc = DateTimeOffset.UtcNow.AddDays(-1),
            EndDateUtc = DateTimeOffset.UtcNow.AddDays(30),
        });
        var projectId = await TestApp.SendAsync(new CreateProjectCommand
        {
            Name = "Projeto Vinculos",
            GoalAmount = 2000,
            Status = "Active",
            CampaignIds = [firstCampaignId, secondCampaignId],
        });

        await TestApp.SendAsync(new UpdateProjectCommand
        {
            Id = projectId,
            Name = "Projeto Vinculos",
            GoalAmount = 2000,
            Status = "Active",
            CampaignIds = [firstCampaignId],
        });
        await TestApp.SendAsync(new UpdateProjectCommand
        {
            Id = projectId,
            Name = "Projeto Vinculos",
            GoalAmount = 2000,
            Status = "Active",
            CampaignIds = [firstCampaignId, secondCampaignId],
        });

        var projects = await TestApp.SendAsync(new GetProjectsQuery
        {
            Search = "Vinculos",
            PageSize = 10,
        });

        var project = projects.Items.Single(item => item.Id == projectId);
        project.Campaigns.Select(campaign => campaign.Id).ShouldBe([firstCampaignId, secondCampaignId], ignoreOrder: true);
    }

    [Test]
    public async Task ShouldGenerateReceiptPrintAndPdf()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Recibo");
        var paidAt = DateTimeOffset.UtcNow;
        var donationId = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            Amount = 120,
            Type = "OneTime",
            Status = "Confirmed",
            PaymentMethod = "Pix",
            PaidAtUtc = paidAt,
            Reference = "REC-SMOKE-001",
        });

        var receiptId = await TestApp.SendAsync(new IssueReceiptCommand(donationId));

        var print = await TestApp.SendAsync(new GetReceiptPrintQuery(receiptId));
        print.ShouldNotBeNull();
        print.DonorName.ShouldBe("Doador Recibo");
        print.Amount.ShouldBe(120);
        print.DonationReference.ShouldBe("REC-SMOKE-001");

        var pdf = await TestApp.SendAsync(new GetReceiptPdfQuery(receiptId));
        pdf.ShouldNotBeNull();
        pdf.FileName.ShouldEndWith(".pdf");
        Encoding.ASCII.GetString(pdf.Content.Take(4).ToArray()).ShouldBe("%PDF");
    }

    [Test]
    public async Task ShouldExportProjectAccountabilityAsCsvAndPdf()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Prestacao");
        var campaignId = await TestApp.SendAsync(new CreateCampaignCommand
        {
            Name = "Campanha Prestacao",
            Type = "Fundraising",
            Channel = "Email",
            GoalAmount = 1000,
            StartDateUtc = DateTimeOffset.UtcNow.AddDays(-1),
            EndDateUtc = DateTimeOffset.UtcNow.AddDays(30),
        });
        var projectId = await TestApp.SendAsync(new CreateProjectCommand
        {
            Name = "Projeto Prestacao",
            GoalAmount = 1000,
            Status = "Active",
            CampaignIds = [campaignId],
        });
        _ = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            CampaignId = campaignId,
            ProjectId = projectId,
            Amount = 75,
            Type = "OneTime",
            Status = "Confirmed",
            PaymentMethod = "Pix",
            PaidAtUtc = DateTimeOffset.UtcNow.AddDays(-10),
            Reference = "PREST-OLD-SMOKE",
        });
        var donationId = await TestApp.SendAsync(new CreateDonationCommand
        {
            DonorId = donorId,
            CampaignId = campaignId,
            ProjectId = projectId,
            Amount = 250,
            Type = "OneTime",
            Status = "Confirmed",
            PaymentMethod = "Pix",
            PaidAtUtc = DateTimeOffset.UtcNow,
            Reference = "PREST-SMOKE-001",
        });
        await TestApp.SendAsync(new IssueReceiptCommand(donationId));

        var filteredReport = await TestApp.SendAsync(new GetProjectAccountabilityQuery(
            projectId,
            campaignId,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1)));
        filteredReport.ShouldNotBeNull();
        filteredReport.FilterCampaignId.ShouldBe(campaignId);
        filteredReport.DonationsCount.ShouldBe(1);
        filteredReport.RaisedAmount.ShouldBe(250);
        filteredReport.Campaigns.Single().DonationsCount.ShouldBe(1);
        filteredReport.Campaigns.Single().DonorsCount.ShouldBe(1);
        filteredReport.Campaigns.Single().AverageDonationAmount.ShouldBe(250);
        filteredReport.Campaigns.Single().SharePercentage.ShouldBe(100);
        filteredReport.Donations.ShouldContain(donation => donation.Reference == "PREST-SMOKE-001");
        filteredReport.Donations.ShouldNotContain(donation => donation.Reference == "PREST-OLD-SMOKE");

        var csv = await TestApp.SendAsync(new ExportProjectAccountabilityQuery(projectId, "csv", null, null, null));
        csv.ShouldNotBeNull();
        csv.ContentType.ShouldBe("text/csv");
        Encoding.UTF8.GetString(csv.Content).ShouldContain("Doador Prestacao");
        Encoding.UTF8.GetString(csv.Content).ShouldContain("PREST-SMOKE-001");

        var pdf = await TestApp.SendAsync(new ExportProjectAccountabilityQuery(projectId, "pdf", null, null, null));
        pdf.ShouldNotBeNull();
        pdf.ContentType.ShouldBe("application/pdf");
        Encoding.ASCII.GetString(pdf.Content.Take(4).ToArray()).ShouldBe("%PDF");
    }

    [Test]
    public async Task ShouldUploadDocumentAttachmentForDonor()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Documento");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("comprovante"));

        var documentId = await TestApp.SendAsync(new CreateDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Comprovante",
            null,
            "Arquivo de teste",
            new FileUpload("comprovante.txt", "text/plain", content, content.Length)));

        var documents = await TestApp.SendAsync(new GetDocumentAttachmentsQuery("Donor", donorId));

        documents.Items.ShouldContain(document =>
            document.Id == documentId &&
            document.Title == "Comprovante" &&
            document.Url == $"/api/DocumentAttachments/{documentId}/Download");
    }

    [Test]
    public async Task ShouldCreateTemporaryAccessUrlForDocumentAttachment()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Link Temporario");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("arquivo com link temporario"));

        var documentId = await TestApp.SendAsync(new CreateDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo com link",
            null,
            null,
            new FileUpload("temporario.txt", "text/plain", content, content.Length)));

        var accessUrl = await TestApp.SendAsync(new CreateDocumentAttachmentAccessUrlQuery(documentId, 5));

        accessUrl.ShouldNotBeNull();
        accessUrl.Url.ShouldStartWith("https://storage.local/");
        accessUrl.ExpiresAtUtc.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task ShouldDownloadAndRemoveDocumentAttachment()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Download");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("arquivo para baixar"));

        var documentId = await TestApp.SendAsync(new CreateDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo download",
            null,
            null,
            new FileUpload("download.txt", "text/plain", content, content.Length)));

        var download = await TestApp.SendAsync(new DownloadDocumentAttachmentQuery(documentId));
        download.ShouldNotBeNull();
        download.FileName.ShouldBe("download.txt");
        using var reader = new StreamReader(download.Content, Encoding.UTF8);
        (await reader.ReadToEndAsync()).ShouldBe("arquivo para baixar");

        await TestApp.SendAsync(new DeleteDocumentAttachmentCommand(documentId));

        var documents = await TestApp.SendAsync(new GetDocumentAttachmentsQuery("Donor", donorId));
        documents.Items.ShouldNotContain(document => document.Id == documentId);
        var removedDownload = await TestApp.SendAsync(new DownloadDocumentAttachmentQuery(documentId));
        removedDownload.ShouldBeNull();
    }

    [Test]
    public async Task ShouldRestrictDocumentRemovalByRole()
    {
        var organizationId = await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Permissao Documento");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("arquivo protegido"));

        var documentId = await TestApp.SendAsync(new CreateDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo protegido",
            null,
            null,
            new FileUpload("protegido.txt", "text/plain", content, content.Length)));

        await TestApp.RunAsUserAsync($"{Guid.NewGuid():N}@local", "Agent1234!", [Roles.Agent]);
        TestApp.SetOrganizationId(organizationId);

        await Should.ThrowAsync<ForbiddenAccessException>(
            async () => await TestApp.SendAsync(new DeleteDocumentAttachmentCommand(documentId)));

        await TestApp.RunAsUserAsync($"{Guid.NewGuid():N}@local", "Manager1234!", [Roles.Manager]);
        TestApp.SetOrganizationId(organizationId);

        await Should.NotThrowAsync(async () => await TestApp.SendAsync(new DeleteDocumentAttachmentCommand(documentId)));
    }

    [Test]
    public async Task ShouldNotReadDocumentAttachmentFromAnotherOrganization()
    {
        var organizationA = await CreateAndUseOrganizationAsync("Organizacao Documento A");
        var donorId = await CreateDonorAsync("Doador Documento Organizacao A");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("arquivo isolado"));

        var documentId = await TestApp.SendAsync(new CreateDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo isolado",
            null,
            null,
            new FileUpload("isolado.txt", "text/plain", content, content.Length)));

        await CreateAndUseOrganizationAsync("Organizacao Documento B");

        var documentsFromOtherOrganization = await TestApp.SendAsync(new GetDocumentAttachmentsQuery("Donor", donorId));
        documentsFromOtherOrganization.Items.ShouldBeEmpty();

        var downloadFromOtherOrganization = await TestApp.SendAsync(new DownloadDocumentAttachmentQuery(documentId));
        downloadFromOtherOrganization.ShouldBeNull();

        TestApp.SetOrganizationId(organizationA);
        var documentsFromOriginalOrganization = await TestApp.SendAsync(new GetDocumentAttachmentsQuery("Donor", donorId));
        documentsFromOriginalOrganization.Items.ShouldContain(document => document.Id == documentId);
    }

    private static async Task<Guid> CreateAndUseOrganizationAsync(string name = "Organizacao Smoke")
    {
        await TestApp.RunAsUserAsync($"{Guid.NewGuid():N}@local", "Administrator1234!", [Roles.SystemAdministrator]);
        var organization = await TestApp.SendAsync(new CreateOrganizationCommand(
            new CreateOrganizationRequest(name, name, null, 1000)));
        TestApp.SetOrganizationId(organization.Id);

        return organization.Id;
    }

    private static async Task<Guid> CreateDonorAsync(string fullName)
    {
        return await TestApp.SendAsync(new CreateDonorCommand
        {
            FullName = fullName,
            PersonType = "Individual",
            Status = "Lead",
            Source = "Manual",
            Email = $"{Guid.NewGuid():N}@example.com",
            Phone = "62999990000",
            City = "Goiania",
            State = "GO",
            AllowsCommunication = true,
        });
    }
}
