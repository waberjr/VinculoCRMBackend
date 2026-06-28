namespace VinculoBackend.Application.Donations.Commands.CreateDonation;

public sealed class CreateDonationCommandValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationCommandValidator()
    {
        RuleFor(v => v.DonorId).NotEmpty();
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.TypeOptionId).NotEmpty();
        RuleFor(v => v.StatusOptionId).NotEmpty();
        RuleFor(v => v.PaymentMethodOptionId).NotEmpty();
        RuleFor(v => v.Reference).MaximumLength(120);
        RuleFor(v => v.ExternalPaymentId).MaximumLength(120);
        RuleFor(v => v.Notes).MaximumLength(1000);
    }
}
