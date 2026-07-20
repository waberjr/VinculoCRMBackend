using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalAlertRuleField2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IgnoreCancelledTasksForAutoResolution",
                table: "OperationalAlertRules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IgnoreCancelledTasksForAutoResolution",
                table: "OperationalAlertRules");
        }
    }
}
