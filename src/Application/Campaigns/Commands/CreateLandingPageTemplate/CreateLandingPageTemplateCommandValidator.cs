namespace VinculoBackend.Application.Campaigns.Commands.CreateLandingPageTemplate;

public sealed class CreateLandingPageTemplateCommandValidator : AbstractValidator<CreateLandingPageTemplateCommand>
{
    public CreateLandingPageTemplateCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(command => command.Category)
            .MaximumLength(80);

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(180);

        RuleFor(command => command.Subtitle)
            .MaximumLength(600);

        RuleFor(command => command.HeroImageUrl)
            .MaximumLength(1000);

        RuleFor(command => command.GoalAmount)
            .GreaterThan(0)
            .When(command => command.GoalAmount is not null);
    }
}
