using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageFields2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "LandingPageTemplates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionLimitMaxAttempts",
                table: "LandingPages",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionLimitWindowMinutes",
                table: "LandingPages",
                type: "int",
                nullable: false,
                defaultValue: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "LandingPageTemplates");

            migrationBuilder.DropColumn(
                name: "SubmissionLimitMaxAttempts",
                table: "LandingPages");

            migrationBuilder.DropColumn(
                name: "SubmissionLimitWindowMinutes",
                table: "LandingPages");
        }
    }
}
