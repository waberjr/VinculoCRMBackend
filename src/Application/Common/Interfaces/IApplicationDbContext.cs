using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<ConfigurableOption> ConfigurableOptions { get; }

    DbSet<Donor> Donors { get; }

    DbSet<DonorTag> DonorTags { get; }

    DbSet<DonorTagAssignment> DonorTagAssignments { get; }

    DbSet<Campaign> Campaigns { get; }

    DbSet<Donation> Donations { get; }

    DbSet<DonationPlan> DonationPlans { get; }

    DbSet<RelationshipTask> RelationshipTasks { get; }

    DbSet<DonorTimelineEntry> DonorTimelineEntries { get; }

    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
