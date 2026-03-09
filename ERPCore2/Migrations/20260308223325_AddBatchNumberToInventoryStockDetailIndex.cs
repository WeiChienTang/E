using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchNumberToInventoryStockDetailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId",
                table: "InventoryStockDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId_BatchNumber",
                table: "InventoryStockDetails",
                columns: new[] { "InventoryStockId", "WarehouseId", "WarehouseLocationId", "BatchNumber" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL AND [BatchNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId_BatchNumber",
                table: "InventoryStockDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId",
                table: "InventoryStockDetails",
                columns: new[] { "InventoryStockId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL");
        }
    }
}
