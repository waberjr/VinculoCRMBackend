namespace VinculoBackend.Application.Communications.Commands.CreateCommunicationCampaign;

public sealed class CreateCommunicationCampaignCommandValidator : AbstractValidator<CreateCommunicationCampaignCommand>
{
    public CreateCommunicationCampaignCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Channel).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Audience).NotEmpty().MaximumLength(500);
        RuleFor(command => command.DonorIds).NotNull();
    }
}
