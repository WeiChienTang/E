using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class MoveStockLevelLimitsToDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStockLevel",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "InventoryStocks");

            migrationBuilder.AddColumn<int>(
                name: "MaxStockLevel",
                table: "InventoryStockDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "InventoryStockDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStockLevel",
                table: "InventoryStockDetails");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "InventoryStockDetails");

            migrationBuilder.AddColumn<int>(
                name: "MaxStockLevel",
                table: "InventoryStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "InventoryStocks",
                type: "int",
                nullable: true);
        }
    }
}
