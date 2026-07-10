using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComunicationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunicationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Channel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true),
                    Body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    Variables = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_CommunicationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationTemplates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Channel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Audience = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ScheduledAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    PlannedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    RecipientsCount = table.Column<int>(type: "int", nullable: false),
                    BlockedByConsentCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CommunicationCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaigns_CommunicationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CommunicationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaigns_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationCampaignRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CommunicationCampaignId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DonorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    BlockReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    TimelineEntryId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_CommunicationCampaignRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaignRecipients_CommunicationCampaigns_Commu~",
                        column: x => x.CommunicationCampaignId,
                        principalTable: "CommunicationCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaignRecipients_DonorTimelineEntries_Timelin~",
                        column: x => x.TimelineEntryId,
                        principalTable: "DonorTimelineEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaignRecipients_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaignRecipients_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaignRecipients_CommunicationCampaignId",
                table: "CommunicationCampaignRecipients",
                column: "CommunicationCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaignRecipients_DonorId",
                table: "CommunicationCampaignRecipients",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaignRecipients_OrganizationId_Communication~",
                table: "CommunicationCampaignRecipients",
                columns: new[] { "OrganizationId", "CommunicationCampaignId", "DonorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaignRecipients_TimelineEntryId",
                table: "CommunicationCampaignRecipients",
                column: "TimelineEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaigns_OrganizationId_Status_PlannedAtUtc",
                table: "CommunicationCampaigns",
                columns: new[] { "OrganizationId", "Status", "PlannedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaigns_TemplateId",
                table: "CommunicationCampaigns",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_OrganizationId_Channel_IsActive",
                table: "CommunicationTemplates",
                columns: new[] { "OrganizationId", "Channel", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationCampaignRecipients");

            migrationBuilder.DropTable(
                name: "CommunicationCampaigns");

            migrationBuilder.DropTable(
                name: "CommunicationTemplates");
        }
    }
}
