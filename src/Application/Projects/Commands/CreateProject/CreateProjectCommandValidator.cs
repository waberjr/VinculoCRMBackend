namespace VinculoBackend.Application.ImpactProjects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(180);
        RuleFor(v => v.Description).MaximumLength(1000);
        RuleFor(v => v.GoalAmount).GreaterThan(0).When(v => v.GoalAmount is not null);
        RuleFor(v => v.ImpactMetric).MaximumLength(500);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(80);
        RuleForEach(v => v.CampaignIds).NotEmpty();
        RuleFor(v => v.EndDateUtc)
            .GreaterThan(v => v.StartDateUtc)
            .When(v => v.StartDateUtc is not null && v.EndDateUtc is not null)
            .WithMessage("A data final deve ser maior que a data inicial.");
    }
}
