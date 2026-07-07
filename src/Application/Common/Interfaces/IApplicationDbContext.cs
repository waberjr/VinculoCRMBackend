using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<OrganizationMember> OrganizationMembers { get; }

    DbSet<OrganizationInvitation> OrganizationInvitations { get; }

    DbSet<ConfigurableOption> ConfigurableOptions { get; }

    DbSet<Donor> Donors { get; }

    DbSet<DonorTag> DonorTags { get; }

    DbSet<DonorTagAssignment> DonorTagAssignments { get; }

    DbSet<DonorPhone> DonorPhones { get; }

    DbSet<DonorEmail> DonorEmails { get; }

    DbSet<Campaign> Campaigns { get; }

    DbSet<Project> Projects { get; }

    DbSet<ProjectCampaign> ProjectCampaigns { get; }

    DbSet<Donation> Donations { get; }

    DbSet<DonationProject> DonationProjects { get; }

    DbSet<DonationPlan> DonationPlans { get; }

    DbSet<RelationshipTask> RelationshipTasks { get; }

    DbSet<DonorTimelineEntry> DonorTimelineEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
