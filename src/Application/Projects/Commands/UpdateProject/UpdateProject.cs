using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.ImpactProjects.Commands.UpdateProject;

public record UpdateProjectCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? GoalAmount { get; init; }
    public string? ImpactMetric { get; init; }
    public string Status { get; init; } = "Draft";
    public DateTimeOffset? StartDateUtc { get; init; }
    public DateTimeOffset? EndDateUtc { get; init; }
}

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public UpdateProjectCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        _ = RequiredOrganization.From(_organizationContext);

        var project = await _context.Projects.FindAsync([request.Id], cancellationToken);
        if (project is null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Project), request.Id.ToString());
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.ImpactMetric = request.ImpactMetric?.Trim();
        project.Status = SystemOptionMapper.Parse<ProjectStatus>(request.Status);
        project.SetGoalAmount(request.GoalAmount);
        project.SetPeriod(request.StartDateUtc, request.EndDateUtc);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
