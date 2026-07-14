namespace VinculoBackend.Application.Campaigns.Commands.UpsertLandingPageConfiguration;

public sealed class UpsertLandingPageConfigurationCommandValidator : AbstractValidator<UpsertLandingPageConfigurationCommand>
{
    public UpsertLandingPageConfigurationCommandValidator()
    {
        RuleFor(command => command.TargetType).NotEmpty().Must(value => value is "campaign" or "project");
        RuleFor(command => command.TargetId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Subtitle).MaximumLength(1000);
        RuleFor(command => command.HeroImageUrl).MaximumLength(1000);
        RuleFor(command => command.GoalAmount).GreaterThan(0).When(command => command.GoalAmount is not null);
    }
}
