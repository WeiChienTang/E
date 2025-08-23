using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSupplierUnitRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSuppliers_Units_UnitId",
                table: "ProductSuppliers",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
