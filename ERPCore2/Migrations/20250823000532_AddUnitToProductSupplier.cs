using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToProductSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "ProductSuppliers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSuppliers_UnitId",
                table: "ProductSuppliers",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers");

            migrationBuilder.DropIndex(
                name: "IX_ProductSuppliers_UnitId",
                table: "ProductSuppliers");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "ProductSuppliers");
        }
    }
}
