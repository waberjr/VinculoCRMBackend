namespace VinculoBackend.Application.Communications.Commands.CancelCommunicationCampaign;

public sealed class CancelCommunicationCampaignCommandValidator : AbstractValidator<CancelCommunicationCampaignCommand>
{
    public CancelCommunicationCampaignCommandValidator()
    {
        RuleFor(command => command.Reason).MaximumLength(500);
    }
}
