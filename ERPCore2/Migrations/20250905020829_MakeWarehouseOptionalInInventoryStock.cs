using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class MakeWarehouseOptionalInInventoryStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "InventoryStocks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks",
                columns: new[] { "ProductId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL AND [WarehouseLocationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "InventoryStocks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks",
                columns: new[] { "ProductId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
