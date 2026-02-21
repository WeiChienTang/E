using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class CleanupProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_PurchaseUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PurchaseUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SupplierId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShowBomOnPrint",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PurchaseUnitConversionRate",
                table: "Products",
                newName: "StandardCost");

            migrationBuilder.AddColumn<decimal>(
                name: "ListPrice",
                table: "Products",
                type: "decimal(18,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListPrice",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "StandardCost",
                table: "Products",
                newName: "PurchaseUnitConversionRate");

            migrationBuilder.AddColumn<int>(
                name: "PurchaseUnitId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowBomOnPrint",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurchaseUnitId",
                table: "Products",
                column: "PurchaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_PurchaseUnitId",
                table: "Products",
                column: "PurchaseUnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
