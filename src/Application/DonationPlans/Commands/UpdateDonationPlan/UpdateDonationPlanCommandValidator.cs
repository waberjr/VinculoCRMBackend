namespace VinculoBackend.Application.DonationPlans.Commands.UpdateDonationPlan;

public sealed class UpdateDonationPlanCommandValidator : AbstractValidator<UpdateDonationPlanCommand>
{
    public UpdateDonationPlanCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.DonorId).NotEmpty();
        RuleFor(v => v.ExpectedAmount).GreaterThan(0);
        RuleFor(v => v.BillingDay).InclusiveBetween(1, 28);
        RuleFor(v => v.PreferredPaymentMethod).NotEmpty().MaximumLength(80);
        RuleFor(v => v.StartDateUtc)
            .NotEmpty()
            .Must(value => value > DateTimeOffset.MinValue)
            .WithMessage("A data de inicio e obrigatoria.");
        RuleFor(v => v.Notes).MaximumLength(1000);
    }
}
