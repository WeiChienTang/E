using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 修改退回欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailRemarks",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsShipped",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ProcessedQuantity",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "QualityCondition",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ReturnUnitPrice",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ScrapQuantity",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ShippedDate",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ShippedQuantity",
                table: "PurchaseReturnDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailRemarks",
                table: "PurchaseReturnDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "PurchaseReturnDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShipped",
                table: "PurchaseReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProcessedQuantity",
                table: "PurchaseReturnDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QualityCondition",
                table: "PurchaseReturnDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnUnitPrice",
                table: "PurchaseReturnDetails",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ScrapQuantity",
                table: "PurchaseReturnDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedDate",
                table: "PurchaseReturnDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippedQuantity",
                table: "PurchaseReturnDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
