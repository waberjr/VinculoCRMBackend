using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_ChannelOptionId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_StatusOptionId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_TypeOptionId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_PreferredPaymentMethodOpt~",
                table: "DonationPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_StatusOptionId",
                table: "DonationPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_ConfigurableOptions_PaymentMethodOptionId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_ConfigurableOptions_StatusOptionId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_ConfigurableOptions_TypeOptionId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorEmails_ConfigurableOptions_TypeOptionId",
                table: "DonorEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorPhones_ConfigurableOptions_TypeOptionId",
                table: "DonorPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_PersonTypeOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorTimelineEntries_ConfigurableOptions_TypeOptionId",
                table: "DonorTimelineEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ConfigurableOptions_ContactOutcomeOptionId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ConfigurableOptions_PriorityOptionId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ConfigurableOptions_StatusOptionId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ConfigurableOptions_TypeOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ContactOutcomeOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_OrganizationId_AssignedUserId_StatusOptionId_DueAtUtc",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_OrganizationId_DonorId_StatusOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TypeOptionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_DonorTimelineEntries_TypeOptionId",
                table: "DonorTimelineEntries");

            migrationBuilder.DropIndex(
                name: "IX_Donors_OrganizationId_StatusOptionId",
                table: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_Donors_PersonTypeOptionId",
                table: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_Donors_StatusOptionId",
                table: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_DonorPhones_TypeOptionId",
                table: "DonorPhones");

            migrationBuilder.DropIndex(
                name: "IX_DonorEmails_TypeOptionId",
                table: "DonorEmails");

            migrationBuilder.DropIndex(
                name: "IX_Donations_OrganizationId_StatusOptionId_ExpectedAtUtc",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_PaymentMethodOptionId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_StatusOptionId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_TypeOptionId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_DonationPlans_OrganizationId_DonorId_StatusOptionId",
                table: "DonationPlans");

            migrationBuilder.DropIndex(
                name: "IX_DonationPlans_PreferredPaymentMethodOptionId",
                table: "DonationPlans");

            migrationBuilder.DropIndex(
                name: "IX_DonationPlans_StatusOptionId",
                table: "DonationPlans");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_ChannelOptionId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_OrganizationId_StatusOptionId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_StatusOptionId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_TypeOptionId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ContactOutcomeOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PriorityOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StatusOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "DonorTimelineEntries");

            migrationBuilder.DropColumn(
                name: "PersonTypeOptionId",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "StatusOptionId",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "DonorPhones");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "DonorEmails");

            migrationBuilder.DropColumn(
                name: "PaymentMethodOptionId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "StatusOptionId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethodOptionId",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "StatusOptionId",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "ChannelOptionId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "StatusOptionId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TypeOptionId",
                table: "Campaigns");

            migrationBuilder.AddColumn<string>(
                name: "ContactOutcome",
                table: "Tasks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Tasks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tasks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Tasks",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DonorTimelineEntries",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonType",
                table: "Donors",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Donors",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DonorPhones",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DonorEmails",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Donations",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Donations",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Donations",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredPaymentMethod",
                table: "DonationPlans",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DonationPlans",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Campaigns",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Campaigns",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Campaigns",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_AssignedUserId_Status_DueAtUtc",
                table: "Tasks",
                columns: new[] { "OrganizationId", "AssignedUserId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_DonorId_Status",
                table: "Tasks",
                columns: new[] { "OrganizationId", "DonorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Status",
                table: "Donors",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_Status_ExpectedAtUtc",
                table: "Donations",
                columns: new[] { "OrganizationId", "Status", "ExpectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_OrganizationId_DonorId_Status",
                table: "DonationPlans",
                columns: new[] { "OrganizationId", "DonorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_Status",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_OrganizationId_AssignedUserId_Status_DueAtUtc",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_OrganizationId_DonorId_Status",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Donors_OrganizationId_Status",
                table: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_Donations_OrganizationId_Status_ExpectedAtUtc",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_DonationPlans_OrganizationId_DonorId_Status",
                table: "DonationPlans");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_OrganizationId_Status",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ContactOutcome",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DonorTimelineEntries");

            migrationBuilder.DropColumn(
                name: "PersonType",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DonorPhones");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DonorEmails");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DonationPlans");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Campaigns");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactOutcomeOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PriorityOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "DonorTimelineEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PersonTypeOptionId",
                table: "Donors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusOptionId",
                table: "Donors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "DonorPhones",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "DonorEmails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodOptionId",
                table: "Donations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusOptionId",
                table: "Donations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "Donations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredPaymentMethodOptionId",
                table: "DonationPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusOptionId",
                table: "DonationPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChannelOptionId",
                table: "Campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusOptionId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeOptionId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ContactOutcomeOptionId",
                table: "Tasks",
                column: "ContactOutcomeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_AssignedUserId_StatusOptionId_DueAtUtc",
                table: "Tasks",
                columns: new[] { "OrganizationId", "AssignedUserId", "StatusOptionId", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OrganizationId_DonorId_StatusOptionId",
                table: "Tasks",
                columns: new[] { "OrganizationId", "DonorId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusOptionId",
                table: "Tasks",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TypeOptionId",
                table: "Tasks",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorTimelineEntries_TypeOptionId",
                table: "DonorTimelineEntries",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_StatusOptionId",
                table: "Donors",
                columns: new[] { "OrganizationId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Donors_PersonTypeOptionId",
                table: "Donors",
                column: "PersonTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_StatusOptionId",
                table: "Donors",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorPhones_TypeOptionId",
                table: "DonorPhones",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorEmails_TypeOptionId",
                table: "DonorEmails",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrganizationId_StatusOptionId_ExpectedAtUtc",
                table: "Donations",
                columns: new[] { "OrganizationId", "StatusOptionId", "ExpectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_PaymentMethodOptionId",
                table: "Donations",
                column: "PaymentMethodOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_StatusOptionId",
                table: "Donations",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_TypeOptionId",
                table: "Donations",
                column: "TypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_OrganizationId_DonorId_StatusOptionId",
                table: "DonationPlans",
                columns: new[] { "OrganizationId", "DonorId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_PreferredPaymentMethodOptionId",
                table: "DonationPlans",
                column: "PreferredPaymentMethodOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPlans_StatusOptionId",
                table: "DonationPlans",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ChannelOptionId",
                table: "Campaigns",
                column: "ChannelOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_StatusOptionId",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "StatusOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_StatusOptionId",
                table: "Campaigns",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_TypeOptionId",
                table: "Campaigns",
                column: "TypeOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_ChannelOptionId",
                table: "Campaigns",
                column: "ChannelOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_StatusOptionId",
                table: "Campaigns",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_TypeOptionId",
                table: "Campaigns",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_PreferredPaymentMethodOpt~",
                table: "DonationPlans",
                column: "PreferredPaymentMethodOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_StatusOptionId",
                table: "DonationPlans",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_PaymentMethodOptionId",
                table: "Donations",
                column: "PaymentMethodOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_StatusOptionId",
                table: "Donations",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_TypeOptionId",
                table: "Donations",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorEmails_ConfigurableOptions_TypeOptionId",
                table: "DonorEmails",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorPhones_ConfigurableOptions_TypeOptionId",
                table: "DonorPhones",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_PersonTypeOptionId",
                table: "Donors",
                column: "PersonTypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                table: "Donors",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorTimelineEntries_ConfigurableOptions_TypeOptionId",
                table: "DonorTimelineEntries",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_ContactOutcomeOptionId",
                table: "Tasks",
                column: "ContactOutcomeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_StatusOptionId",
                table: "Tasks",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_TypeOptionId",
                table: "Tasks",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
