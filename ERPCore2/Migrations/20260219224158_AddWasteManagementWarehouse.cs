using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddWasteManagementWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WasteTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "WasteRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseLocationId",
                table: "WasteRecords",
                type: "int",
                nullable: true);

            // 將既有的 WasteRecords 更新為第一個有效倉庫，避免 FK 約束衝突
            migrationBuilder.Sql(@"
                UPDATE [WasteRecords]
                SET [WarehouseId] = (SELECT TOP 1 [Id] FROM [Warehouses] ORDER BY [Id])
                WHERE EXISTS (SELECT 1 FROM [Warehouses])");

            migrationBuilder.CreateIndex(
                name: "IX_WasteTypes_ProductId",
                table: "WasteTypes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WasteRecords_WarehouseId",
                table: "WasteRecords",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WasteRecords_WarehouseLocationId",
                table: "WasteRecords",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteRecords_WarehouseLocations_WarehouseLocationId",
                table: "WasteRecords",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteRecords_Warehouses_WarehouseId",
                table: "WasteRecords",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteTypes_Products_ProductId",
                table: "WasteTypes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteRecords_WarehouseLocations_WarehouseLocationId",
                table: "WasteRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteRecords_Warehouses_WarehouseId",
                table: "WasteRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteTypes_Products_ProductId",
                table: "WasteTypes");

            migrationBuilder.DropIndex(
                name: "IX_WasteTypes_ProductId",
                table: "WasteTypes");

            migrationBuilder.DropIndex(
                name: "IX_WasteRecords_WarehouseId",
                table: "WasteRecords");

            migrationBuilder.DropIndex(
                name: "IX_WasteRecords_WarehouseLocationId",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WasteTypes");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationId",
                table: "WasteRecords");
        }
    }
}
