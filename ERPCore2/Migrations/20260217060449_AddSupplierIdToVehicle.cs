using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierIdToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SupplierId",
                table: "Vehicles",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Suppliers_SupplierId",
                table: "Vehicles",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Suppliers_SupplierId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_SupplierId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Vehicles");
        }
    }
}
