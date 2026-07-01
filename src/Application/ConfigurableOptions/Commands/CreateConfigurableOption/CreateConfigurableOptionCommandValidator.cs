namespace VinculoBackend.Application.ConfigurableOptions.Commands.CreateConfigurableOption;

public sealed class CreateConfigurableOptionCommandValidator : AbstractValidator<CreateConfigurableOptionCommand>
{
    public CreateConfigurableOptionCommandValidator()
    {
        RuleFor(v => v.Category).IsInEnum();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(120);
        RuleFor(v => v.Description).MaximumLength(500);
        RuleFor(v => v.Color).MaximumLength(32);
    }
}
