namespace VinculoBackend.Application.RelationshipTasks.Commands.CreateTasksFromSegment;

public sealed class CreateTasksFromSegmentCommandValidator : AbstractValidator<CreateTasksFromSegmentCommand>
{
    public CreateTasksFromSegmentCommandValidator()
    {
        RuleFor(command => command.Segment).NotEmpty().MaximumLength(80);
        RuleFor(command => command.Title).NotEmpty().MaximumLength(180);
        RuleFor(command => command.Description).MaximumLength(1000);
        RuleFor(command => command.Type).NotEmpty().MaximumLength(80);
        RuleFor(command => command.Priority).NotEmpty().MaximumLength(80);
        RuleFor(command => command.DueAtUtc).NotNull();
        RuleFor(command => command.Limit).InclusiveBetween(1, 500);
    }
}
