namespace VinculoBackend.Application.Communications.Commands.CreateCommunicationTemplate;

public sealed class CreateCommunicationTemplateCommandValidator : AbstractValidator<CreateCommunicationTemplateCommand>
{
    public CreateCommunicationTemplateCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Channel).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Subject).MaximumLength(180);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(4000);
    }
}
