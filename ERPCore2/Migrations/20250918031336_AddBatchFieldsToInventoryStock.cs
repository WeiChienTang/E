using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchFieldsToInventoryStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks");

            migrationBuilder.AddColumn<DateTime>(
                name: "BatchDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "InventoryStocks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId_BatchNumber",
                table: "InventoryStocks",
                columns: new[] { "ProductId", "WarehouseId", "WarehouseLocationId", "BatchNumber" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL AND [WarehouseLocationId] IS NOT NULL AND [BatchNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId_BatchNumber",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "BatchDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "InventoryStocks");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks",
                columns: new[] { "ProductId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL AND [WarehouseLocationId] IS NOT NULL");
        }
    }
}
