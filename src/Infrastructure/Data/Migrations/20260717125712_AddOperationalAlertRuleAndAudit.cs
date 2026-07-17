using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalAlertRuleAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalAlertAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OperationalAlertId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Action = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
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
                    table.PrimaryKey("PK_OperationalAlertAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalAlertAuditEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OperationalAlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Source = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WarningThreshold = table.Column<int>(type: "int", nullable: false),
                    HighThreshold = table.Column<int>(type: "int", nullable: false),
                    DueInHours = table.Column<int>(type: "int", nullable: false),
                    AssignedUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
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
                    table.PrimaryKey("PK_OperationalAlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalAlertRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAlertAuditEntries_OrganizationId_OperationalAlert~",
                table: "OperationalAlertAuditEntries",
                columns: new[] { "OrganizationId", "OperationalAlertId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAlertRules_OrganizationId_Source",
                table: "OperationalAlertRules",
                columns: new[] { "OrganizationId", "Source" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalAlertAuditEntries");

            migrationBuilder.DropTable(
                name: "OperationalAlertRules");
        }
    }
}
