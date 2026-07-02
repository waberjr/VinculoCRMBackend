using VinculoBackend.Application.Common.Models;

namespace VinculoBackend.Application.RelationshipTasks.Commands.CompleteRelationshipTask;

public sealed class CompleteRelationshipTaskCommandValidator : AbstractValidator<CompleteRelationshipTaskCommand>
{
    public CompleteRelationshipTaskCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Outcome).NotEmpty().MaximumLength(80);
        RuleFor(v => v.CompletionNote).NotEmpty().MaximumLength(1000);
        RuleFor(v => v.FollowUpAtUtc)
            .NotNull()
            .When(v => ConfigurableOptionCode.FromName(v.Outcome ?? string.Empty) == "requested-callback")
            .WithMessage("A data de retorno e obrigatoria.");
        RuleFor(v => v.DoNotContactReason)
            .NotEmpty()
            .When(v => ConfigurableOptionCode.FromName(v.Outcome ?? string.Empty) == "do-not-contact")
            .WithMessage("A justificativa do bloqueio e obrigatoria.");
    }
}
