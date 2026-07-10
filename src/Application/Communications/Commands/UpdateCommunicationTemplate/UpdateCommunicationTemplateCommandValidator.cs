namespace VinculoBackend.Application.Communications.Commands.UpdateCommunicationTemplate;

public sealed class UpdateCommunicationTemplateCommandValidator : AbstractValidator<UpdateCommunicationTemplateCommand>
{
    public UpdateCommunicationTemplateCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Channel).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Subject).MaximumLength(180);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(4000);
    }
}
