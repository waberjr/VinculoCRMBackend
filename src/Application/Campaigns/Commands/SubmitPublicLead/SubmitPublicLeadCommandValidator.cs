namespace VinculoBackend.Application.Campaigns.Commands.SubmitPublicLead;

public sealed class SubmitPublicLeadCommandValidator : AbstractValidator<SubmitPublicLeadCommand>
{
    public SubmitPublicLeadCommandValidator()
    {
        RuleFor(command => command.TargetType).NotEmpty().Must(value => value is "campaign" or "project");
        RuleFor(command => command.TargetId).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Email).MaximumLength(180).EmailAddress().When(command => !string.IsNullOrWhiteSpace(command.Email));
        RuleFor(command => command.Phone).MaximumLength(40);
        RuleFor(command => command.DonationAmount).GreaterThan(0).LessThanOrEqualTo(9999999999).When(command => command.DonationAmount is not null);
        RuleFor(command => command.Source).MaximumLength(120);
        RuleFor(command => command.UtmSource).MaximumLength(120);
        RuleFor(command => command.UtmMedium).MaximumLength(120);
        RuleFor(command => command.UtmCampaign).MaximumLength(120);
        RuleFor(command => command.UtmContent).MaximumLength(120);
        RuleFor(command => command.UtmTerm).MaximumLength(120);
        RuleFor(command => command).Must(command => !string.IsNullOrWhiteSpace(command.Email) || !string.IsNullOrWhiteSpace(command.Phone))
            .WithMessage("Informe e-mail ou telefone.");
    }
}
