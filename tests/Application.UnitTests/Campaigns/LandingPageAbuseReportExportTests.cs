using System.Text;
using MediatR;
using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.ExportLandingPageAbuseReport;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageAbuseReport;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class LandingPageAbuseReportExportTests
{
    [Test]
    public async Task HandleShouldExportCsvWithFilteredItems()
    {
        var sender = new Mock<ISender>();
        var pdfExporter = new Mock<ILandingPageAbuseReportPdfExporter>();
        var targetId = Guid.NewGuid();
        sender
            .Setup(service => service.Send(
                It.Is<GetLandingPageAbuseReportQuery>(query => query.TargetType == "campaign" && query.TargetId == targetId && query.Blocked == true && query.Limit == 1000),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LandingPageAbuseReportDto
            {
                AttemptsCount = 2,
                BlockedCount = 1,
                Items =
                [
                    new LandingPageAbuseReportItemDto
                    {
                        Id = Guid.NewGuid(),
                        TargetType = "campaign",
                        TargetId = targetId,
                        TargetName = "Campanha Julho",
                        Source = "qr",
                        Blocked = true,
                        Reason = "fingerprint-rate-limit",
                        AttemptedAtUtc = new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.Zero),
                    },
                ],
            });

        var handler = new ExportLandingPageAbuseReportQueryHandler(sender.Object, pdfExporter.Object);
        var result = await handler.Handle(new ExportLandingPageAbuseReportQuery("csv", "campaign", targetId, Blocked: true), CancellationToken.None);

        var csv = Encoding.UTF8.GetString(result.Content);
        result.ContentType.ShouldBe("text/csv");
        csv.ShouldContain("Resumo,Tentativas,Bloqueadas");
        csv.ShouldContain("Campanha Julho");
        csv.ShouldContain("fingerprint-rate-limit");
        pdfExporter.Verify(service => service.Generate(It.IsAny<LandingPageAbuseReportDto>()), Times.Never);
    }

    [Test]
    public async Task HandleShouldUsePdfExporterForPdf()
    {
        var sender = new Mock<ISender>();
        var pdfExporter = new Mock<ILandingPageAbuseReportPdfExporter>();
        var expectedContent = new byte[] { 8, 9 };
        sender
            .Setup(service => service.Send(It.IsAny<GetLandingPageAbuseReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LandingPageAbuseReportDto());
        pdfExporter
            .Setup(service => service.Generate(It.IsAny<LandingPageAbuseReportDto>()))
            .Returns(expectedContent);

        var handler = new ExportLandingPageAbuseReportQueryHandler(sender.Object, pdfExporter.Object);
        var result = await handler.Handle(new ExportLandingPageAbuseReportQuery("pdf"), CancellationToken.None);

        result.ContentType.ShouldBe("application/pdf");
        result.Content.ShouldBe(expectedContent);
    }
}
