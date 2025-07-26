using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RenamePurchaseReceiptToPurchaseReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 重新命名外鍵約束
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceiptDetails_PurchaseReceipts_PurchaseReceiptId",
                table: "PurchaseReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceiptDetails_Products_ProductId",
                table: "PurchaseReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceiptDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceiptDetails_WarehouseLocations_WarehouseLocationId",
                table: "PurchaseReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceipts_Employees_ConfirmedBy",
                table: "PurchaseReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceipts_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceipts_Warehouses_WarehouseId",
                table: "PurchaseReceipts");

            // 重新命名表格
            migrationBuilder.RenameTable(
                name: "PurchaseReceipts",
                newName: "PurchaseReceivings");

            migrationBuilder.RenameTable(
                name: "PurchaseReceiptDetails",
                newName: "PurchaseReceivingDetails");

            // 重新命名索引
            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceipts_WarehouseId",
                table: "PurchaseReceivings",
                newName: "IX_PurchaseReceivings_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceipts_ReceiptStatus_ReceiptDate",
                table: "PurchaseReceivings",
                newName: "IX_PurchaseReceivings_ReceiptStatus_ReceiptDate");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceipts_PurchaseOrderId_ReceiptDate",
                table: "PurchaseReceivings",
                newName: "IX_PurchaseReceivings_PurchaseOrderId_ReceiptDate");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceipts_ReceiptNumber",
                table: "PurchaseReceivings",
                newName: "IX_PurchaseReceivings_ReceiptNumber");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceipts_ConfirmedBy",
                table: "PurchaseReceivings",
                newName: "IX_PurchaseReceivings_ConfirmedBy");

            // 重新命名欄位
            migrationBuilder.RenameColumn(
                name: "PurchaseReceiptId",
                table: "PurchaseReceivingDetails",
                newName: "PurchaseReceivingId");

            // 重新命名索引
            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceiptDetails_WarehouseLocationId",
                table: "PurchaseReceivingDetails",
                newName: "IX_PurchaseReceivingDetails_WarehouseLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceiptDetails_PurchaseReceiptId_ProductId",
                table: "PurchaseReceivingDetails",
                newName: "IX_PurchaseReceivingDetails_PurchaseReceivingId_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceiptDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                newName: "IX_PurchaseReceivingDetails_PurchaseOrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceiptDetails_ProductId",
                table: "PurchaseReceivingDetails",
                newName: "IX_PurchaseReceivingDetails_ProductId");

            // 重新建立外鍵約束
            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseReceivings_PurchaseReceivingId",
                table: "PurchaseReceivingDetails",
                column: "PurchaseReceivingId",
                principalTable: "PurchaseReceivings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_Products_ProductId",
                table: "PurchaseReceivingDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                column: "PurchaseOrderDetailId",
                principalTable: "PurchaseOrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivingDetails_WarehouseLocations_WarehouseLocationId",
                table: "PurchaseReceivingDetails",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Employees_ConfirmedBy",
                table: "PurchaseReceivings",
                column: "ConfirmedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceivings",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Warehouses_WarehouseId",
                table: "PurchaseReceivings",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 刪除外鍵約束
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseReceivings_PurchaseReceivingId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_Products_ProductId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivingDetails_WarehouseLocations_WarehouseLocationId",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Employees_ConfirmedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceivings");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Warehouses_WarehouseId",
                table: "PurchaseReceivings");

            // 重新命名欄位
            migrationBuilder.RenameColumn(
                name: "PurchaseReceivingId",
                table: "PurchaseReceivingDetails",
                newName: "PurchaseReceiptId");

            // 重新命名表格
            migrationBuilder.RenameTable(
                name: "PurchaseReceivings",
                newName: "PurchaseReceipts");

            migrationBuilder.RenameTable(
                name: "PurchaseReceivingDetails",
                newName: "PurchaseReceiptDetails");

            // 重新命名索引
            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivings_WarehouseId",
                table: "PurchaseReceipts",
                newName: "IX_PurchaseReceipts_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivings_ReceiptStatus_ReceiptDate",
                table: "PurchaseReceipts",
                newName: "IX_PurchaseReceipts_ReceiptStatus_ReceiptDate");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivings_PurchaseOrderId_ReceiptDate",
                table: "PurchaseReceipts",
                newName: "IX_PurchaseReceipts_PurchaseOrderId_ReceiptDate");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivings_ReceiptNumber",
                table: "PurchaseReceipts",
                newName: "IX_PurchaseReceipts_ReceiptNumber");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivings_ConfirmedBy",
                table: "PurchaseReceipts",
                newName: "IX_PurchaseReceipts_ConfirmedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivingDetails_WarehouseLocationId",
                table: "PurchaseReceiptDetails",
                newName: "IX_PurchaseReceiptDetails_WarehouseLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivingDetails_PurchaseReceivingId_ProductId",
                table: "PurchaseReceiptDetails",
                newName: "IX_PurchaseReceiptDetails_PurchaseReceiptId_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivingDetails_PurchaseOrderDetailId",
                table: "PurchaseReceiptDetails",
                newName: "IX_PurchaseReceiptDetails_PurchaseOrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReceivingDetails_ProductId",
                table: "PurchaseReceiptDetails",
                newName: "IX_PurchaseReceiptDetails_ProductId");

            // 重新建立外鍵約束
            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceiptDetails_PurchaseReceipts_PurchaseReceiptId",
                table: "PurchaseReceiptDetails",
                column: "PurchaseReceiptId",
                principalTable: "PurchaseReceipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceiptDetails_Products_ProductId",
                table: "PurchaseReceiptDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceiptDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                table: "PurchaseReceiptDetails",
                column: "PurchaseOrderDetailId",
                principalTable: "PurchaseOrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceiptDetails_WarehouseLocations_WarehouseLocationId",
                table: "PurchaseReceiptDetails",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceipts_Employees_ConfirmedBy",
                table: "PurchaseReceipts",
                column: "ConfirmedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceipts_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceipts",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceipts_Warehouses_WarehouseId",
                table: "PurchaseReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
