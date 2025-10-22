using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryStockProductRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Products_ProductId1",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId1",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "InventoryStocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "InventoryStocks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId1",
                table: "InventoryStocks",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Products_ProductId1",
                table: "InventoryStocks",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
