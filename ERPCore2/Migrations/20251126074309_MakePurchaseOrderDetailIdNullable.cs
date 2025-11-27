using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class MakePurchaseOrderDetailIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                column: "PurchaseOrderDetailId",
                principalTable: "PurchaseOrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                column: "PurchaseOrderDetailId",
                principalTable: "PurchaseOrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
