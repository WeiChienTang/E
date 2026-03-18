using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalPeriodAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "FiscalPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosedByEmployeeId",
                table: "FiscalPeriods",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReopenReason",
                table: "FiscalPeriods",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReopenedAt",
                table: "FiscalPeriods",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "FiscalPeriods");

            migrationBuilder.DropColumn(
                name: "ClosedByEmployeeId",
                table: "FiscalPeriods");

            migrationBuilder.DropColumn(
                name: "ReopenReason",
                table: "FiscalPeriods");

            migrationBuilder.DropColumn(
                name: "ReopenedAt",
                table: "FiscalPeriods");
        }
    }
}
