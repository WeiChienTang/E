using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWarehouseFromPurchaseReceivingAndAddToPurchaseReceivingDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Warehouses_WarehouseId",
                table: "PurchaseReceivings");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_WarehouseId",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PurchaseReceivings");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "PurchaseReceivingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_WarehouseId",
                table: "PurchaseReceivingDetails",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_Warehouses_WarehouseId",
                table: "PurchaseReceivingDetails",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_Warehouses_WarehouseId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivingDetails_WarehouseId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "PurchaseReceivings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_WarehouseId",
                table: "PurchaseReceivings",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Warehouses_WarehouseId",
                table: "PurchaseReceivings",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
