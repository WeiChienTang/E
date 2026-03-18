using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialIssueJournalizationAndProductionCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "ProductionScheduleCompletions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "ProductionScheduleCompletions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "MaterialIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedByEmployeeId",
                table: "MaterialIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "MaterialIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "MaterialIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IssueType",
                table: "MaterialIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "MaterialIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_ConfirmedByEmployeeId",
                table: "MaterialIssues",
                column: "ConfirmedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_IsConfirmed",
                table: "MaterialIssues",
                column: "IsConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_IsJournalized",
                table: "MaterialIssues",
                column: "IsJournalized");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssues_Employees_ConfirmedByEmployeeId",
                table: "MaterialIssues",
                column: "ConfirmedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssues_Employees_ConfirmedByEmployeeId",
                table: "MaterialIssues");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssues_ConfirmedByEmployeeId",
                table: "MaterialIssues");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssues_IsConfirmed",
                table: "MaterialIssues");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssues_IsJournalized",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "ProductionScheduleCompletions");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "ProductionScheduleCompletions");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "ConfirmedByEmployeeId",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "MaterialIssues");
        }
    }
}
