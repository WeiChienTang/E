using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierToPurchaseReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "PurchaseReceivings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PurchaseReceivings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_SupplierId",
                table: "PurchaseReceivings",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Suppliers_SupplierId",
                table: "PurchaseReceivings",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Suppliers_SupplierId",
                table: "PurchaseReceivings");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_SupplierId",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PurchaseReceivings");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "PurchaseReceivings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
