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
            .When(v => v.Outcome == "RequestedCallback")
            .WithMessage("A data de retorno e obrigatoria.");
        RuleFor(v => v.DoNotContactReason)
            .NotEmpty()
            .When(v => v.Outcome == "DoNotContact")
            .WithMessage("A justificativa do bloqueio e obrigatoria.");
    }
}
