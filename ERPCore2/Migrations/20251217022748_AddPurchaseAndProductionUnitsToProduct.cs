using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseAndProductionUnitsToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductionUnitId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseUnitId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductionUnitId",
                table: "Products",
                column: "ProductionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurchaseUnitId",
                table: "Products",
                column: "PurchaseUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_ProductionUnitId",
                table: "Products",
                column: "ProductionUnitId",
                principalTable: "Units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_PurchaseUnitId",
                table: "Products",
                column: "PurchaseUnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_ProductionUnitId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_PurchaseUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductionUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PurchaseUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductionUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseUnitId",
                table: "Products");
        }
    }
}
