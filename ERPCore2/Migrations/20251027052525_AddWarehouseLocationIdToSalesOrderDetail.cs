using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseLocationIdToSalesOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseLocationId",
                table: "SalesOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_WarehouseLocationId",
                table: "SalesOrderDetails",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetails_WarehouseLocations_WarehouseLocationId",
                table: "SalesOrderDetails",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetails_WarehouseLocations_WarehouseLocationId",
                table: "SalesOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetails_WarehouseLocationId",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationId",
                table: "SalesOrderDetails");
        }
    }
}
