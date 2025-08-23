using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductSupplierID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_PrimarySupplierId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PrimarySupplierId",
                table: "Products",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_PrimarySupplierId",
                table: "Products",
                newName: "IX_Products_SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "Products",
                newName: "PrimarySupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                newName: "IX_Products_PrimarySupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_PrimarySupplierId",
                table: "Products",
                column: "PrimarySupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
