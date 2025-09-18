using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseToSalesOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "SalesOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_WarehouseId",
                table: "SalesOrderDetails",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetails_Warehouses_WarehouseId",
                table: "SalesOrderDetails",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetails_Warehouses_WarehouseId",
                table: "SalesOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetails_WarehouseId",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "SalesOrderDetails");
        }
    }
}
