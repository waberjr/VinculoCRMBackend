namespace VinculoBackend.Application.DonorTags.Commands.CreateDonorTag;

public sealed class CreateDonorTagCommandValidator : AbstractValidator<CreateDonorTagCommand>
{
    public CreateDonorTagCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Description).MaximumLength(250);
    }
}
