namespace VinculoBackend.Application.RelationshipTasks.Commands.UpdateRelationshipTask;

public sealed class UpdateRelationshipTaskCommandValidator : AbstractValidator<UpdateRelationshipTaskCommand>
{
    public UpdateRelationshipTaskCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.DonorId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(180);
        RuleFor(v => v.Description).MaximumLength(1000);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Priority).NotEmpty().MaximumLength(80);
        RuleFor(v => v.DueAtUtc).NotNull();
        RuleFor(v => v.BlockedContactJustification).MaximumLength(500);
    }
}
