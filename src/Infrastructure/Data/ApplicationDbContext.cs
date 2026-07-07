using System.Reflection;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Domain.Entities;
using VinculoBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VinculoBackend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IOrganizationContext _organizationContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOrganizationContext organizationContext) : base(options)
    {
        _organizationContext = organizationContext;
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();

    public DbSet<ConfigurableOption> ConfigurableOptions => Set<ConfigurableOption>();

    public DbSet<Donor> Donors => Set<Donor>();

    public DbSet<DonorTag> DonorTags => Set<DonorTag>();

    public DbSet<DonorTagAssignment> DonorTagAssignments => Set<DonorTagAssignment>();

    public DbSet<DonorPhone> DonorPhones => Set<DonorPhone>();

    public DbSet<DonorEmail> DonorEmails => Set<DonorEmail>();

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Donation> Donations => Set<Donation>();

    public DbSet<DonationPlan> DonationPlans => Set<DonationPlan>();

    public DbSet<RelationshipTask> RelationshipTasks => Set<RelationshipTask>();

    public DbSet<DonorTimelineEntry> DonorTimelineEntries => Set<DonorTimelineEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<OrganizationMember>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<OrganizationInvitation>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ConfigurableOption>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<Donor>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonorTag>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonorTagAssignment>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonorPhone>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonorEmail>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<Campaign>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<Donation>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonationPlan>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<RelationshipTask>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
        builder.Entity<DonorTimelineEntry>().HasQueryFilter(e => !e.IsDeleted && (!_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId));
    }
}
