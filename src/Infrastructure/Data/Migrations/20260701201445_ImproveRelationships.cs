using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinculoBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveRelationships : Migration
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
                name: "FK_Donors_ConfigurableOptions_PreferredContactChannelOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_RelationshipProfileOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_SourceOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorTagAssignments_DonorTags_DonorTagId",
                table: "DonorTagAssignments");

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
                name: "IX_Donors_OrganizationId_Document",
                table: "Donors");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Document",
                table: "Donors",
                columns: new[] { "OrganizationId", "Document" },
                unique: true,
                filter: "\"Document\" IS NOT NULL");

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
                name: "FK_Donors_ConfigurableOptions_PreferredContactChannelOptionId",
                table: "Donors",
                column: "PreferredContactChannelOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_RelationshipProfileOptionId",
                table: "Donors",
                column: "RelationshipProfileOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_SourceOptionId",
                table: "Donors",
                column: "SourceOptionId",
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
                name: "FK_DonorTagAssignments_DonorTags_DonorTagId",
                table: "DonorTagAssignments",
                column: "DonorTagId",
                principalTable: "DonorTags",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_Donors_ConfigurableOptions_PreferredContactChannelOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_RelationshipProfileOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_SourceOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorTagAssignments_DonorTags_DonorTagId",
                table: "DonorTagAssignments");

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
                name: "IX_Donors_OrganizationId_Document",
                table: "Donors");

            migrationBuilder.CreateIndex(
                name: "IX_Donors_OrganizationId_Document",
                table: "Donors",
                columns: new[] { "OrganizationId", "Document" },
                filter: "\"Document\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_ChannelOptionId",
                table: "Campaigns",
                column: "ChannelOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_StatusOptionId",
                table: "Campaigns",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ConfigurableOptions_TypeOptionId",
                table: "Campaigns",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_PreferredPaymentMethodOpt~",
                table: "DonationPlans",
                column: "PreferredPaymentMethodOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationPlans_ConfigurableOptions_StatusOptionId",
                table: "DonationPlans",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_PaymentMethodOptionId",
                table: "Donations",
                column: "PaymentMethodOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_StatusOptionId",
                table: "Donations",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_ConfigurableOptions_TypeOptionId",
                table: "Donations",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorEmails_ConfigurableOptions_TypeOptionId",
                table: "DonorEmails",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorPhones_ConfigurableOptions_TypeOptionId",
                table: "DonorPhones",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_PersonTypeOptionId",
                table: "Donors",
                column: "PersonTypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_PreferredContactChannelOptionId",
                table: "Donors",
                column: "PreferredContactChannelOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_RelationshipProfileOptionId",
                table: "Donors",
                column: "RelationshipProfileOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_SourceOptionId",
                table: "Donors",
                column: "SourceOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_ConfigurableOptions_StatusOptionId",
                table: "Donors",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorTagAssignments_DonorTags_DonorTagId",
                table: "DonorTagAssignments",
                column: "DonorTagId",
                principalTable: "DonorTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorTimelineEntries_ConfigurableOptions_TypeOptionId",
                table: "DonorTimelineEntries",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_ContactOutcomeOptionId",
                table: "Tasks",
                column: "ContactOutcomeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_PriorityOptionId",
                table: "Tasks",
                column: "PriorityOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_StatusOptionId",
                table: "Tasks",
                column: "StatusOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ConfigurableOptions_TypeOptionId",
                table: "Tasks",
                column: "TypeOptionId",
                principalTable: "ConfigurableOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
