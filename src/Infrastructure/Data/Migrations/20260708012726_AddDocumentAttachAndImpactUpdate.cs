using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAttachAndImpactUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptNumberNextSequence",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumberPrefix",
                table: "Organizations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
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
                    table.PrimaryKey("PK_DocumentAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAttachments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImpactUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
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
                    table.PrimaryKey("PK_ImpactUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImpactUpdates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImpactUpdates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAttachments_OrganizationId_EntityType_EntityId",
                table: "DocumentAttachments",
                columns: new[] { "OrganizationId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImpactUpdates_OrganizationId_ProjectId_PublishedAtUtc",
                table: "ImpactUpdates",
                columns: new[] { "OrganizationId", "ProjectId", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImpactUpdates_ProjectId",
                table: "ImpactUpdates",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentAttachments");

            migrationBuilder.DropTable(
                name: "ImpactUpdates");

            migrationBuilder.DropColumn(
                name: "ReceiptNumberNextSequence",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ReceiptNumberPrefix",
                table: "Organizations");
        }
    }
}
