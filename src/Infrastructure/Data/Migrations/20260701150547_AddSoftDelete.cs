using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_DonorTags_OrganizationId_Name",
                table: "DonorTags");

            migrationBuilder.DropIndex(
                name: "IX_DonorTagAssignments_OrganizationId_DonorId_DonorTagId",
                table: "DonorTagAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_Code",
                table: "ConfigurableOptions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "TodoLists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TodoLists",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TodoLists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TodoItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TodoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "Organizations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "OrganizationMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrganizationMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrganizationMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "OrganizationInvitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrganizationInvitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrganizationInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonorTimelineEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonorTimelineEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonorTimelineEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonorTags",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonorTags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonorTags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonorTagAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonorTagAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonorTagAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "Donors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Donors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Donors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonorPhones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonorPhones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonorPhones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonorEmails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonorEmails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonorEmails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "Donations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Donations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Donations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "DonationPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DonationPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DonationPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "ConfigurableOptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ConfigurableOptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ConfigurableOptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                table: "OrganizationMembers",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTags_OrganizationId_Name",
                table: "DonorTags",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTagAssignments_OrganizationId_DonorId_DonorTagId",
                table: "DonorTagAssignments",
                columns: new[] { "OrganizationId", "DonorId", "DonorTagId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_Code",
                table: "ConfigurableOptions",
                columns: new[] { "OrganizationId", "Category", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_DonorTags_OrganizationId_Name",
                table: "DonorTags");

            migrationBuilder.DropIndex(
                name: "IX_DonorTagAssignments_OrganizationId_DonorId_DonorTagId",
                table: "DonorTagAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_Code",
                table: "ConfigurableOptions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "OrganizationMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrganizationMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrganizationMembers");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "OrganizationInvitations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrganizationInvitations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrganizationInvitations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonorTimelineEntries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonorTimelineEntries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonorTimelineEntries");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonorTags");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonorTags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonorTags");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonorTagAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonorTagAssignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonorTagAssignments");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonorPhones");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonorPhones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonorPhones");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonorEmails");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonorEmails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonorEmails");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ConfigurableOptions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ConfigurableOptions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ConfigurableOptions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                table: "OrganizationMembers",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorTags_OrganizationId_Name",
                table: "DonorTags",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorTagAssignments_OrganizationId_DonorId_DonorTagId",
                table: "DonorTagAssignments",
                columns: new[] { "OrganizationId", "DonorId", "DonorTagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurableOptions_OrganizationId_Category_Code",
                table: "ConfigurableOptions",
                columns: new[] { "OrganizationId", "Category", "Code" },
                unique: true);
        }
    }
}
