using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonationProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DonationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false),
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
                    table.PrimaryKey("PK_DonationProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationProjects_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationProjects_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DonationProjects_DonationId",
                table: "DonationProjects",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationProjects_OrganizationId_DonationId_ProjectId",
                table: "DonationProjects",
                columns: new[] { "OrganizationId", "DonationId", "ProjectId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DonationProjects_OrganizationId_ProjectId",
                table: "DonationProjects",
                columns: new[] { "OrganizationId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationProjects_ProjectId",
                table: "DonationProjects",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonationProjects");
        }
    }
}
