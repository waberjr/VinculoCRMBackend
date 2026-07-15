using System.Text;
using MediatR;
using Moq;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Campaigns.Models;
using VinculoBackend.Application.Campaigns.Queries.ExportLandingPageLeads;
using VinculoBackend.Application.Campaigns.Queries.GetLandingPageLeads;
using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.UnitTests.Campaigns;

public class ExportLandingPageLeadsTests
{
    [Test]
    public async Task HandleShouldExportAllFilteredPages()
    {
        var sender = new Mock<ISender>();
        var targetId = Guid.NewGuid();

        sender
            .Setup(service => service.Send(It.Is<GetLandingPageLeadsQuery>(query => query.PageNumber == 1 && query.Source == "crm"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<LandingPageLeadDto>
            {
                Items =
                [
                    new LandingPageLeadDto
                    {
                        DonorId = Guid.NewGuid(),
                        DonorName = "Ana",
                        Email = "ana@example.com",
                        Source = "crm",
                        CreatedAtUtc = new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero),
                    },
                ],
                PageNumber = 1,
                PageSize = 100,
                TotalPages = 2,
                TotalCount = 2,
            });
        sender
            .Setup(service => service.Send(It.Is<GetLandingPageLeadsQuery>(query => query.PageNumber == 2 && query.Source == "crm"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<LandingPageLeadDto>
            {
                Items =
                [
                    new LandingPageLeadDto
                    {
                        DonorId = Guid.NewGuid(),
                        DonorName = "Bruno",
                        Phone = "11999999999",
                        Source = "crm",
                        PromisedAmount = 50,
                        DonationStatus = "Pending",
                        CreatedAtUtc = new DateTimeOffset(2026, 7, 14, 13, 0, 0, TimeSpan.Zero),
                    },
                ],
                PageNumber = 2,
                PageSize = 100,
                TotalPages = 2,
                TotalCount = 2,
            });

        var handler = new ExportLandingPageLeadsQueryHandler(sender.Object);
        var result = await handler.Handle(new ExportLandingPageLeadsQuery("campaign", targetId, Source: "crm"), CancellationToken.None);

        var csv = Encoding.UTF8.GetString(result.Content);
        result.ContentType.ShouldBe("text/csv");
        csv.ShouldContain("Ana");
        csv.ShouldContain("Bruno");
        csv.ShouldContain("Pending");
        sender.Verify(service => service.Send(It.IsAny<GetLandingPageLeadsQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
