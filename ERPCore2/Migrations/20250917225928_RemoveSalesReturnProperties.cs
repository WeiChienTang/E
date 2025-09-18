using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSalesReturnProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_ReturnStatus_ReturnDate",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ActualProcessDate",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ExpectedProcessDate",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ProcessPersonnel",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ProcessRemarks",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "RefundRemarks",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ReturnDescription",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "SalesReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualProcessDate",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedProcessDate",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessPersonnel",
                table: "SalesReturns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessRemarks",
                table: "SalesReturns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "SalesReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RefundRemarks",
                table: "SalesReturns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnDescription",
                table: "SalesReturns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnStatus",
                table: "SalesReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ReturnStatus_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "ReturnStatus", "ReturnDate" });
        }
    }
}
