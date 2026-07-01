namespace VinculoBackend.Application.Campaigns.Commands.CreateCampaign;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(180);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Channel).MaximumLength(80);
        RuleFor(v => v.GoalAmount).NotNull().GreaterThan(0);
        RuleFor(v => v.StartDateUtc).NotNull();
        RuleFor(v => v.EndDateUtc).NotNull();
        RuleFor(v => v.EndDateUtc)
            .GreaterThan(v => v.StartDateUtc)
            .When(v => v.StartDateUtc is not null && v.EndDateUtc is not null)
            .WithMessage("A data final deve ser maior que a data inicial.");
        RuleFor(v => v.Description).MaximumLength(1000);
    }
}
