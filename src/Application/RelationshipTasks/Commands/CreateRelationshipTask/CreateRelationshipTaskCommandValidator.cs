namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;

public sealed class CreateRelationshipTaskCommandValidator : AbstractValidator<CreateRelationshipTaskCommand>
{
    public CreateRelationshipTaskCommandValidator()
    {
        RuleFor(v => v)
            .Must(v => v.DonorId is not null || v.OperationalAlertId is not null)
            .WithMessage("Informe um doador ou um alerta operacional para criar a tarefa.");
        RuleFor(v => v.Title).NotEmpty().MaximumLength(180);
        RuleFor(v => v.Description).MaximumLength(1000);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Priority).NotEmpty().MaximumLength(80);
        RuleFor(v => v.DueAtUtc).NotNull();
        RuleFor(v => v.BlockedContactJustification).MaximumLength(500);
    }
}
