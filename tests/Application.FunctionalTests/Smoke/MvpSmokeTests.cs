using System;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using VinculoBackend.Application.DocumentAttachments.Commands.UploadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Commands.DeleteDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Queries.DownloadDocumentAttachment;
using VinculoBackend.Application.DocumentAttachments.Queries.GetDocumentAttachments;
using VinculoBackend.Application.Common.Exceptions;
using System.Threading.Tasks;
using VinculoBackend.Application.Dashboard.Queries.GetDashboardOverview;
using VinculoBackend.Application.Donations.Commands.ConfirmDonation;
using VinculoBackend.Application.Donations.Commands.CreateDonation;
using VinculoBackend.Application.Donors.Commands.CreateDonor;
using VinculoBackend.Application.Donors.Queries.FindDonorDuplicates;
using VinculoBackend.Application.Donors.Queries.GetDonorById;
using VinculoBackend.Application.Organizations.Commands.CreateOrganization;
using VinculoBackend.Application.Organizations.Models;
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
    public async Task ShouldUploadDocumentAttachmentForDonor()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Documento");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("comprovante"));

        var documentId = await TestApp.SendAsync(new UploadDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Comprovante",
            "Arquivo de teste",
            "comprovante.txt",
            "text/plain",
            content,
            content.Length));

        var documents = await TestApp.SendAsync(new GetDocumentAttachmentsQuery("Donor", donorId));

        documents.Items.ShouldContain(document =>
            document.Id == documentId &&
            document.Title == "Comprovante" &&
            document.Url == $"/api/DocumentAttachments/{documentId}/Download");
    }

    [Test]
    public async Task ShouldDownloadAndRemoveDocumentAttachment()
    {
        await CreateAndUseOrganizationAsync();
        var donorId = await CreateDonorAsync("Doador Download");
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("arquivo para baixar"));

        var documentId = await TestApp.SendAsync(new UploadDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo download",
            null,
            "download.txt",
            "text/plain",
            content,
            content.Length));

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

        var documentId = await TestApp.SendAsync(new UploadDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo protegido",
            null,
            "protegido.txt",
            "text/plain",
            content,
            content.Length));

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

        var documentId = await TestApp.SendAsync(new UploadDocumentAttachmentCommand(
            "Donor",
            donorId,
            "Arquivo isolado",
            null,
            "isolado.txt",
            "text/plain",
            content,
            content.Length));

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
