using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.Donations.Commands.CreateDonation;

public sealed class CreateDonationCommandValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationCommandValidator()
    {
        RuleFor(v => v.DonorId).NotEmpty();
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(80);
        RuleFor(v => v.PaymentMethod).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Reference).MaximumLength(120);
        RuleFor(v => v.ExternalPaymentId).MaximumLength(120);
        RuleFor(v => v.Notes).MaximumLength(1000);
        RuleFor(v => v.PaidAtUtc)
            .NotNull()
            .When(v => ConfigurableOptionCode.FromName(v.Status) == "confirmed")
            .WithMessage("A data de pagamento e obrigatória para contribuições confirmadas.");
        RuleFor(v => v.ExpectedAtUtc)
            .NotNull()
            .When(v => ConfigurableOptionCode.FromName(v.Status) == "pending")
            .WithMessage("A data esperada e obrigatória para contribuições pendentes.");
    }
}
