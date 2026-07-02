using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Enums;

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
            .When(v => !string.IsNullOrWhiteSpace(v.Outcome) && SystemOptionMapper.Parse<ContactOutcome>(v.Outcome) == ContactOutcome.RequestedCallback)
            .WithMessage("A data de retorno e obrigatoria.");
        RuleFor(v => v.DoNotContactReason)
            .NotEmpty()
            .When(v => !string.IsNullOrWhiteSpace(v.Outcome) && SystemOptionMapper.Parse<ContactOutcome>(v.Outcome) == ContactOutcome.DoNotContact)
            .WithMessage("A justificativa do bloqueio e obrigatoria.");
    }
}
