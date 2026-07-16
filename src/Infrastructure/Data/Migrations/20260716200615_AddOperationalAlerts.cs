using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingPageBlockRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TargetType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    TargetId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FingerprintHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    Source = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Reason = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
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
                    table.PrimaryKey("PK_LandingPageBlockRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageBlockRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LandingPageTemplateVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Category = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    Title = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Subtitle = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    HeroImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    GoalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    CustomFieldsJson = table.Column<string>(type: "json", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
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
                    table.PrimaryKey("PK_LandingPageTemplateVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageTemplateVersions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OperationalAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ActionUrl = table.Column<string>(type: "varchar(360)", maxLength: 360, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    AcknowledgedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    ResolutionNote = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
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
                    table.PrimaryKey("PK_OperationalAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalAlerts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageBlockRules_OrganizationId_TargetType_TargetId_Fin~",
                table: "LandingPageBlockRules",
                columns: new[] { "OrganizationId", "TargetType", "TargetId", "FingerprintHash", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageBlockRules_OrganizationId_TargetType_TargetId_IsA~",
                table: "LandingPageBlockRules",
                columns: new[] { "OrganizationId", "TargetType", "TargetId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageTemplateVersions_OrganizationId_TemplateId_Version",
                table: "LandingPageTemplateVersions",
                columns: new[] { "OrganizationId", "TemplateId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAlerts_OrganizationId_Source_RelatedEntityType_Re~",
                table: "OperationalAlerts",
                columns: new[] { "OrganizationId", "Source", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAlerts_OrganizationId_Status_Severity_OccurredAtU~",
                table: "OperationalAlerts",
                columns: new[] { "OrganizationId", "Status", "Severity", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageBlockRules");

            migrationBuilder.DropTable(
                name: "LandingPageTemplateVersions");

            migrationBuilder.DropTable(
                name: "OperationalAlerts");
        }
    }
}
