using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipTaskAndOrganizationMemberFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OperationalSlaHours",
                table: "OrganizationMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperationalTaskGoalMonthly",
                table: "OrganizationMembers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationalSlaHours",
                table: "OrganizationMembers");

            migrationBuilder.DropColumn(
                name: "OperationalTaskGoalMonthly",
                table: "OrganizationMembers");
        }
    }
}
