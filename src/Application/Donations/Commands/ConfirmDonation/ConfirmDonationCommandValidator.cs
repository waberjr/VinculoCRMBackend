namespace VinculoBackend.Application.Donations.Commands.ConfirmDonation;

public sealed class ConfirmDonationCommandValidator : AbstractValidator<ConfirmDonationCommand>
{
    public ConfirmDonationCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.PaidAtUtc)
            .NotEmpty()
            .Must(value => value > DateTimeOffset.MinValue)
            .WithMessage("A data de pagamento e obrigatoria.");
        RuleFor(v => v.Reference).MaximumLength(120);
    }
}
