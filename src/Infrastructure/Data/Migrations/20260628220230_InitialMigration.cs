using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Document = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DefaultMonthlyGoal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Colour_Code = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurableOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurableOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurableOptions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonorTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorTags_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Done = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_TodoLists_ListId",
                        column: x => x.ListId,
                        principalTable: "TodoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    StartDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_ConfigurableOptions_ChannelOptionId",
                        column: x => x.ChannelOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Campaigns_ConfigurableOptions_StatusOptionId",
                        column: x => x.StatusOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Campaigns_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Campaigns_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    PersonTypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Document = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    WhatsApp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    State = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AddressLine1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AddressLine2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelationshipProfileOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreferredContactChannelOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowsCommunication = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotContact = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotContactReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AcquisitionCampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donors_Campaigns_AcquisitionCampaignId",
                        column: x => x.AcquisitionCampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donors_ConfigurableOptions_PersonTypeOptionId",
                        column: x => x.PersonTypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donors_ConfigurableOptions_PreferredContactChannelOptionId",
                        column: x => x.PreferredContactChannelOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donors_ConfigurableOptions_RelationshipProfileOptionId",
                        column: x => x.RelationshipProfileOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donors_ConfigurableOptions_SourceOptionId",
                        column: x => x.SourceOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                        column: x => x.StatusOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donors_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonationPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ExpectedAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PreferredPaymentMethodOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingDay = table.Column<int>(type: "integer", nullable: false),
                    StartDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PausedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationPlans_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonationPlans_ConfigurableOptions_PreferredPaymentMethodOpt~",
                        column: x => x.PreferredPaymentMethodOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationPlans_ConfigurableOptions_StatusOptionId",
                        column: x => x.StatusOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationPlans_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationPlans_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonorTagAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorTagId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorTagAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorTagAssignments_DonorTags_DonorTagId",
                        column: x => x.DonorTagId,
                        principalTable: "DonorTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorTagAssignments_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorTagAssignments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonorTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorTimelineEntries_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorTimelineEntries_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorTimelineEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    DonationPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RefundedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ExternalPaymentId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefundReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_ConfigurableOptions_PaymentMethodOptionId",
                        column: x => x.PaymentMethodOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donations_ConfigurableOptions_StatusOptionId",
                        column: x => x.StatusOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donations_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donations_DonationPlans_DonationPlanId",
                        column: x => x.DonationPlanId,
                        principalTable: "DonationPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Donations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    DonationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriorityOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ContactOutcomeOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_ConfigurableOptions_ContactOutcomeOptionId",
                        column: x => x.ContactOutcomeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_ConfigurableOptions_PriorityOptionId",
                        column: x => x.PriorityOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_ConfigurableOptions_StatusOptionId",
                        column: x => x.StatusOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ChannelOptionId",
                table: "Campaigns",
                column: "ChannelOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_Name",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_StatusOptionId",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_StatusOptionId",
                table: "Campaigns",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_TypeOptionId",
                table: "Campaigns",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_Code",
                table: "ConfigurableOptions",
                columns: new[] { "OrganizationId", "Category", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_IsActive",
                table: "ConfigurableOptions",
                columns: new[] { "OrganizationId", "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_CampaignId",
                table: "DonationPlans",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_DonorId",
                table: "DonationPlans",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_OrganizationId_DonorId_StatusOptionId",
                table: "DonationPlans",
                columns: new[] { "OrganizationId", "DonorId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_PreferredPaymentMethodOptionId",
                table: "DonationPlans",
                column: "PreferredPaymentMethodOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_StatusOptionId",
                table: "DonationPlans",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CampaignId",
                table: "Donations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonationPlanId",
                table: "Donations",
                column: "DonationPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorId",
                table: "Donations",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_CampaignId_PaidAtUtc",
                table: "Donations",
                columns: new[] { "OrganizationId", "CampaignId", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_DonationPlanId",
                table: "Donations",
                columns: new[] { "OrganizationId", "DonationPlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_DonorId_PaidAtUtc",
                table: "Donations",
                columns: new[] { "OrganizationId", "DonorId", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_ExternalPaymentId",
                table: "Donations",
                columns: new[] { "OrganizationId", "ExternalPaymentId" },
                unique: true,
                filter: "\"ExternalPaymentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_StatusOptionId_ExpectedAtUtc",
                table: "Donations",
                columns: new[] { "OrganizationId", "StatusOptionId", "ExpectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_PaymentMethodOptionId",
                table: "Donations",
                column: "PaymentMethodOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_StatusOptionId",
                table: "Donations",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_TypeOptionId",
                table: "Donations",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_AcquisitionCampaignId",
                table: "Donors",
                column: "AcquisitionCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_AssignedUserId",
                table: "Donors",
                columns: new[] { "OrganizationId", "AssignedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Document",
                table: "Donors",
                columns: new[] { "OrganizationId", "Document" },
                filter: "\"Document\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Email",
                table: "Donors",
                columns: new[] { "OrganizationId", "Email" },
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_FullName",
                table: "Donors",
                columns: new[] { "OrganizationId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Phone",
                table: "Donors",
                columns: new[] { "OrganizationId", "Phone" },
                filter: "\"Phone\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_StatusOptionId",
                table: "Donors",
                columns: new[] { "OrganizationId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Donors_PersonTypeOptionId",
                table: "Donors",
                column: "PersonTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_PreferredContactChannelOptionId",
                table: "Donors",
                column: "PreferredContactChannelOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_RelationshipProfileOptionId",
                table: "Donors",
                column: "RelationshipProfileOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_SourceOptionId",
                table: "Donors",
                column: "SourceOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_StatusOptionId",
                table: "Donors",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTagAssignments_DonorId",
                table: "DonorTagAssignments",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTagAssignments_DonorTagId",
                table: "DonorTagAssignments",
                column: "DonorTagId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTagAssignments_OrganizationId_DonorId_DonorTagId",
                table: "DonorTagAssignments",
                columns: new[] { "OrganizationId", "DonorId", "DonorTagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorTags_OrganizationId_Name",
                table: "DonorTags",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorTimelineEntries_DonorId",
                table: "DonorTimelineEntries",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTimelineEntries_OrganizationId_DonorId_OccurredAtUtc",
                table: "DonorTimelineEntries",
                columns: new[] { "OrganizationId", "DonorId", "OccurredAtUtc" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DonorTimelineEntries_TypeOptionId",
                table: "DonorTimelineEntries",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CampaignId",
                table: "Tasks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ContactOutcomeOptionId",
                table: "Tasks",
                column: "ContactOutcomeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DonationId",
                table: "Tasks",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DonorId",
                table: "Tasks",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_AssignedUserId_StatusOptionId_DueAtUtc",
                table: "Tasks",
                columns: new[] { "OrganizationId", "AssignedUserId", "StatusOptionId", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_DonorId_StatusOptionId",
                table: "Tasks",
                columns: new[] { "OrganizationId", "DonorId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusOptionId",
                table: "Tasks",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TypeOptionId",
                table: "Tasks",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ListId",
                table: "TodoItems",
                column: "ListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DonorTagAssignments");

            migrationBuilder.DropTable(
                name: "DonorTimelineEntries");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DonorTags");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "TodoLists");

            migrationBuilder.DropTable(
                name: "DonationPlans");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "ConfigurableOptions");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
