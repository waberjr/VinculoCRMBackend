namespace VinculoBackend.Application.Communications.Commands.UpdateCommunicationCampaign;

public sealed class UpdateCommunicationCampaignCommandValidator : AbstractValidator<UpdateCommunicationCampaignCommand>
{
    public UpdateCommunicationCampaignCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Channel).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Audience).NotEmpty().MaximumLength(500);
        RuleFor(command => command.DonorIds).NotNull();
    }
}
