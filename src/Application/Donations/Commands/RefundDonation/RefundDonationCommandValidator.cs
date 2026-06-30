namespace VinculoBackend.Application.Donations.Commands.RefundDonation;

public sealed class RefundDonationCommandValidator : AbstractValidator<RefundDonationCommand>
{
    public RefundDonationCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Reason).NotEmpty().MaximumLength(500);
    }
}
