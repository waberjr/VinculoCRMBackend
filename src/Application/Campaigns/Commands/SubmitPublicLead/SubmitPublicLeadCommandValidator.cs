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
        RuleFor(command => command).Must(command => !string.IsNullOrWhiteSpace(command.Email) || !string.IsNullOrWhiteSpace(command.Phone))
            .WithMessage("Informe e-mail ou telefone.");
    }
}
