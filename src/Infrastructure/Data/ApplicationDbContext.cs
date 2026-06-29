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

    public DbSet<ConfigurableOption> ConfigurableOptions => Set<ConfigurableOption>();

    public DbSet<Donor> Donors => Set<Donor>();

    public DbSet<DonorTag> DonorTags => Set<DonorTag>();

    public DbSet<DonorTagAssignment> DonorTagAssignments => Set<DonorTagAssignment>();

    public DbSet<DonorPhone> DonorPhones => Set<DonorPhone>();

    public DbSet<DonorEmail> DonorEmails => Set<DonorEmail>();

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<Donation> Donations => Set<Donation>();

    public DbSet<DonationPlan> DonationPlans => Set<DonationPlan>();

    public DbSet<RelationshipTask> RelationshipTasks => Set<RelationshipTask>();

    public DbSet<DonorTimelineEntry> DonorTimelineEntries => Set<DonorTimelineEntry>();

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<ConfigurableOption>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<Donor>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonorTag>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonorTagAssignment>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonorPhone>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonorEmail>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<Campaign>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<Donation>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonationPlan>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<RelationshipTask>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
        builder.Entity<DonorTimelineEntry>().HasQueryFilter(e => !_organizationContext.HasOrganization || e.OrganizationId == _organizationContext.OrganizationId);
    }
}
