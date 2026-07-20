using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipTaskFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Donors_DonorId",
                table: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "DonorId",
                table: "Tasks",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<Guid>(
                name: "OperationalAlertId",
                table: "Tasks",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_OperationalAlertId",
                table: "Tasks",
                columns: new[] { "OrganizationId", "OperationalAlertId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Donors_DonorId",
                table: "Tasks",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Donors_DonorId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_OrganizationId_OperationalAlertId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "OperationalAlertId",
                table: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "DonorId",
                table: "Tasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Donors_DonorId",
                table: "Tasks",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
