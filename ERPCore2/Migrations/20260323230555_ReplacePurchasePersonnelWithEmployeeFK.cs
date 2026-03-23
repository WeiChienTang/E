using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePurchasePersonnelWithEmployeeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchasePersonnel",
                table: "PurchaseOrders");

            migrationBuilder.AddColumn<int>(
                name: "PurchasePersonnelId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchasePersonnelId",
                table: "PurchaseOrders",
                column: "PurchasePersonnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Employees_PurchasePersonnelId",
                table: "PurchaseOrders",
                column: "PurchasePersonnelId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Employees_PurchasePersonnelId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_PurchasePersonnelId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "PurchasePersonnelId",
                table: "PurchaseOrders");

            migrationBuilder.AddColumn<string>(
                name: "PurchasePersonnel",
                table: "PurchaseOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
