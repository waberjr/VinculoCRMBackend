namespace VinculoBackend.Application.DonationPlans.Commands.CancelDonationPlan;

public sealed class CancelDonationPlanCommandValidator : AbstractValidator<CancelDonationPlanCommand>
{
    public CancelDonationPlanCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Reason).NotEmpty().MaximumLength(500);
    }
}
