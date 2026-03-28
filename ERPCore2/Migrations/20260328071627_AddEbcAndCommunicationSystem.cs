using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddEbcAndCommunicationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultLicenseAlertDays",
                table: "SystemParameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "JournalEntries",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Employees",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AccountingAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    PerformedByName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApproverAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApproverEmployeeId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproverAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApproverAssignments_Employees_ApproverEmployeeId",
                        column: x => x.ApproverEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmployeeId = table.Column<int>(type: "int", nullable: false),
                    SenderEmployeeId = table.Column<int>(type: "int", nullable: true),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    NavigationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemNotifications_Employees_RecipientEmployeeId",
                        column: x => x.RecipientEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemNotifications_Employees_SenderEmployeeId",
                        column: x => x.SenderEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAuditLogs_ActionType",
                table: "AccountingAuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAuditLogs_EntityType_EntityId",
                table: "AccountingAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAuditLogs_PerformedAt",
                table: "AccountingAuditLogs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAuditLogs_PerformedByEmployeeId",
                table: "AccountingAuditLogs",
                column: "PerformedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApproverAssignments_ApproverEmployeeId",
                table: "ApproverAssignments",
                column: "ApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApproverAssignments_ModuleName",
                table: "ApproverAssignments",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotifications_RecipientEmployeeId_CreatedAt",
                table: "SystemNotifications",
                columns: new[] { "RecipientEmployeeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotifications_RecipientEmployeeId_IsRead_CreatedAt",
                table: "SystemNotifications",
                columns: new[] { "RecipientEmployeeId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotifications_SenderEmployeeId",
                table: "SystemNotifications",
                column: "SenderEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingAuditLogs");

            migrationBuilder.DropTable(
                name: "ApproverAssignments");

            migrationBuilder.DropTable(
                name: "SystemNotifications");

            migrationBuilder.DropColumn(
                name: "DefaultLicenseAlertDays",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "JournalEntries");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Employees",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);
        }
    }
}
