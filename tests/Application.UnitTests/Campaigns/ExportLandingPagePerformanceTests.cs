using System.Text;
using MediatR;
using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Queries.ExportLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPagePerformance;
using VinculoBackend.Application.Campaigns.Services;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class ExportLandingPagePerformanceTests
{
    [Test]
    public async Task HandleShouldExportCsvWithDailyMetrics()
    {
        var sender = new Mock<ISender>();
        var pdfExporter = new Mock<ILandingPagePerformancePdfExporter>();
        sender
            .Setup(service => service.Send(It.IsAny<GetLandingPagePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LandingPagePerformanceDto
            {
                ViewsCount = 10,
                LeadsCount = 3,
                Daily =
                [
                    new LandingDailyPerformanceDto
                    {
                        Date = "2026-07-15",
                        ViewsCount = 8,
                        LeadsCount = 2,
                    },
                ],
            });

        var handler = new ExportLandingPagePerformanceQueryHandler(sender.Object, pdfExporter.Object);
        var result = await handler.Handle(new ExportLandingPagePerformanceQuery("csv"), CancellationToken.None);

        var csv = Encoding.UTF8.GetString(result.Content);
        result.ContentType.ShouldBe("text/csv");
        csv.ShouldContain("Resumo,Visualizacoes,Leads");
        csv.ShouldContain("2026-07-15,8,2");
        pdfExporter.Verify(service => service.Generate(It.IsAny<LandingPagePerformanceDto>()), Times.Never);
    }

    [Test]
    public async Task HandleShouldUsePdfExporterForPdf()
    {
        var sender = new Mock<ISender>();
        var pdfExporter = new Mock<ILandingPagePerformancePdfExporter>();
        var expectedContent = new byte[] { 1, 2, 3 };
        sender
            .Setup(service => service.Send(It.IsAny<GetLandingPagePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LandingPagePerformanceDto());
        pdfExporter
            .Setup(service => service.Generate(It.IsAny<LandingPagePerformanceDto>()))
            .Returns(expectedContent);

        var handler = new ExportLandingPagePerformanceQueryHandler(sender.Object, pdfExporter.Object);
        var result = await handler.Handle(new ExportLandingPagePerformanceQuery("pdf"), CancellationToken.None);

        result.ContentType.ShouldBe("application/pdf");
        result.Content.ShouldBe(expectedContent);
    }
}
