namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateRelationshipTask;

public sealed class CreateRelationshipTaskCommandValidator : AbstractValidator<CreateRelationshipTaskCommand>
{
    public CreateRelationshipTaskCommandValidator()
    {
        RuleFor(v => v.DonorId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(180);
        RuleFor(v => v.Description).MaximumLength(1000);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Priority).NotEmpty().MaximumLength(80);
    }
}
