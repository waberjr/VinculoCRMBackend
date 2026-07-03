using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using VinculoBackend.Application.Common.Behaviours;
using VinculoBackend.Application.Donations.Commands.CreateDonation;
using VinculoBackend.Application.Donors.Commands.CreateDonor;
using VinculoBackend.Application.Users.Commands.LoginUser;

namespace VinculoBackend.Application.UnitTests.Common.Behaviours;

public class RequestLogSanitizerTests
{
    [Test]
    public void ShouldRedactLoginPassword()
    {
        var result = RequestLogSanitizer.Sanitize(new LoginUserCommand("user@example.com", "SuperSecret123!"));

        result["Email"].ShouldBe("[Redacted]");
        result["Password"].ShouldBe("[Redacted]");
        result.Values.ShouldNotContain("SuperSecret123!");
    }

    [Test]
    public void ShouldRedactDonorPersonalData()
    {
        var result = RequestLogSanitizer.Sanitize(new CreateDonorCommand
        {
            FullName = "Maria Silva",
            Document = "12345678909",
            Email = "maria@example.com",
            Phone = "62999998888",
            Notes = "Prefere contato pela manha",
            Status = "Lead",
        });

        result["FullName"].ShouldBe("[Redacted]");
        result["Document"].ShouldBe("[Redacted]");
        result["Email"].ShouldBe("[Redacted]");
        result["Phone"].ShouldBe("[Redacted]");
        result["Notes"].ShouldBe("[Redacted]");
        result["Status"].ShouldBe("Lead");
    }

    [Test]
    public void ShouldRedactDonationFinancialDataAndKeepIds()
    {
        var donorId = Guid.NewGuid();
        var result = RequestLogSanitizer.Sanitize(new CreateDonationCommand
        {
            DonorId = donorId,
            Amount = 250,
            Status = "Pending",
            Type = "OneTime",
            PaymentMethod = "Pix",
            Reference = "Comprovante 123",
        });

        result["DonorId"].ShouldBe(donorId);
        result["Amount"].ShouldBe("[Redacted]");
        result["Reference"].ShouldBe("[Redacted]");
        result["Status"].ShouldBe("Pending");
        result["Type"].ShouldBe("OneTime");
        result["PaymentMethod"].ShouldBe("Pix");
    }

    [Test]
    public void ShouldLogCollectionCountOnly()
    {
        var result = RequestLogSanitizer.Sanitize(new RequestWithCollection(["a", "b"]));

        var count = result["Items"]?.GetType().GetProperty("Count")?.GetValue(result["Items"]);
        count.ShouldBe(2);
    }

    private sealed record RequestWithCollection(IReadOnlyCollection<string> Items);
}
