using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhonesAndEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonorEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorEmails_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorEmails_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorEmails_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonorPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorPhones_ConfigurableOptions_TypeOptionId",
                        column: x => x.TypeOptionId,
                        principalTable: "ConfigurableOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorPhones_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonorPhones_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonorEmails_DonorId",
                table: "DonorEmails",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorEmails_OrganizationId_Address",
                table: "DonorEmails",
                columns: new[] { "OrganizationId", "Address" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorEmails_OrganizationId_DonorId",
                table: "DonorEmails",
                columns: new[] { "OrganizationId", "DonorId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorEmails_TypeOptionId",
                table: "DonorEmails",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorPhones_DonorId",
                table: "DonorPhones",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorPhones_OrganizationId_DonorId",
                table: "DonorPhones",
                columns: new[] { "OrganizationId", "DonorId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorPhones_OrganizationId_Number",
                table: "DonorPhones",
                columns: new[] { "OrganizationId", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorPhones_TypeOptionId",
                table: "DonorPhones",
                column: "TypeOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonorEmails");

            migrationBuilder.DropTable(
                name: "DonorPhones");
        }
    }
}
