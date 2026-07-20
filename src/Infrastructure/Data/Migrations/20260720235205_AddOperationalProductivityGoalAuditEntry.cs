using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalProductivityGoalAuditEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalProductivityGoalAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                    PreviousTaskGoalMonthly = table.Column<int>(type: "int", nullable: true),
                    NewTaskGoalMonthly = table.Column<int>(type: "int", nullable: true),
                    PreviousSlaHours = table.Column<int>(type: "int", nullable: true),
                    NewSlaHours = table.Column<int>(type: "int", nullable: true),
                    ChangedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
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
                    table.PrimaryKey("PK_OperationalProductivityGoalAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalProductivityGoalAuditEntries_Organizations_Organi~",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalProductivityGoalAuditEntries_OrganizationId_UserI~",
                table: "OperationalProductivityGoalAuditEntries",
                columns: new[] { "OrganizationId", "UserId", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalProductivityGoalAuditEntries");
        }
    }
}
