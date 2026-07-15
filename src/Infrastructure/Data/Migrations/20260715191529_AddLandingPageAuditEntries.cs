using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingPageAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Action = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Deleted = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageAuditEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LandingPageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Subtitle = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    HeroImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    GoalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    CustomFieldsJson = table.Column<string>(type: "json", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Deleted = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageTemplates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LandingPageViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TargetType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    TargetId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FingerprintHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    UtmSource = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    UtmMedium = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    UtmCampaign = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    WindowStartedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ViewedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Deleted = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageViews_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageAuditEntries_OrganizationId_EntityType_EntityId_O~",
                table: "LandingPageAuditEntries",
                columns: new[] { "OrganizationId", "EntityType", "EntityId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageTemplates_OrganizationId_IsActive_Name",
                table: "LandingPageTemplates",
                columns: new[] { "OrganizationId", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageViews_OrganizationId_TargetType_TargetId_Fingerpr~",
                table: "LandingPageViews",
                columns: new[] { "OrganizationId", "TargetType", "TargetId", "FingerprintHash", "WindowStartedAtUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageViews_OrganizationId_TargetType_TargetId_ViewedAt~",
                table: "LandingPageViews",
                columns: new[] { "OrganizationId", "TargetType", "TargetId", "ViewedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageAuditEntries");

            migrationBuilder.DropTable(
                name: "LandingPageTemplates");

            migrationBuilder.DropTable(
                name: "LandingPageViews");
        }
    }
}
