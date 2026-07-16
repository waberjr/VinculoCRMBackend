using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageSubmissionAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "LandingPageTemplates",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedTemplateId",
                table: "LandingPages",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "LandingPageAuditEntries",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LandingPageSubmissionAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TargetType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    TargetId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FingerprintHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Blocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true),
                    AttemptedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
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
                    table.PrimaryKey("PK_LandingPageSubmissionAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageSubmissionAttempts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPages_OrganizationId_AppliedTemplateId",
                table: "LandingPages",
                columns: new[] { "OrganizationId", "AppliedTemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageAuditEntries_OrganizationId_CreatedByUserId_Occur~",
                table: "LandingPageAuditEntries",
                columns: new[] { "OrganizationId", "CreatedByUserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageSubmissionAttempts_OrganizationId_TargetType_Targ~",
                table: "LandingPageSubmissionAttempts",
                columns: new[] { "OrganizationId", "TargetType", "TargetId", "FingerprintHash", "AttemptedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageSubmissionAttempts");

            migrationBuilder.DropIndex(
                name: "IX_LandingPages_OrganizationId_AppliedTemplateId",
                table: "LandingPages");

            migrationBuilder.DropIndex(
                name: "IX_LandingPageAuditEntries_OrganizationId_CreatedByUserId_Occur~",
                table: "LandingPageAuditEntries");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "LandingPageTemplates");

            migrationBuilder.DropColumn(
                name: "AppliedTemplateId",
                table: "LandingPages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LandingPageAuditEntries");
        }
    }
}
