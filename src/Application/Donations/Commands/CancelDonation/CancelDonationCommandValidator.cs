namespace VinculoBackend.Application.Donations.Commands.CancelDonation;

public sealed class CancelDonationCommandValidator : AbstractValidator<CancelDonationCommand>
{
    public CancelDonationCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Reason).NotEmpty().MaximumLength(500);
    }
}
