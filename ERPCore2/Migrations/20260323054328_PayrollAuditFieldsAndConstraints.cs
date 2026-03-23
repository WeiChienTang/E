using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class PayrollAuditFieldsAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "PayrollRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedBy",
                table: "PayrollRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "MonthlyAttendanceSummaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockedBy",
                table: "MonthlyAttendanceSummaries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaries_EmployeeId_ExpiryDate",
                table: "EmployeeSalaries",
                columns: new[] { "EmployeeId", "ExpiryDate" },
                unique: true,
                filter: "[ExpiryDate] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaries_EmployeeId_ExpiryDate",
                table: "EmployeeSalaries");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "MonthlyAttendanceSummaries");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "MonthlyAttendanceSummaries");
        }
    }
}
