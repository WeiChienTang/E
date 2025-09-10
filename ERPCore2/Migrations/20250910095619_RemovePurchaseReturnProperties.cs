using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemovePurchaseReturnProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_ReturnStatus_ReturnDate",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ProcessCompletedAt",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ProcessRemarks",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "RefundRemarks",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReturnDescription",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "PurchaseReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessCompletedAt",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessRemarks",
                table: "PurchaseReturns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundRemarks",
                table: "PurchaseReturns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnDescription",
                table: "PurchaseReturns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnReason",
                table: "PurchaseReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnStatus",
                table: "PurchaseReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ReturnStatus_ReturnDate",
                table: "PurchaseReturns",
                columns: new[] { "ReturnStatus", "ReturnDate" });
        }
    }
}
