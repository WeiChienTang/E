using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHeaderFKFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceivings");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseReceivings_PurchaseReceivingId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_SalesOrders_SalesOrderId",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Quotations_QuotationId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_SalesDeliveryId_ReturnDate",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_QuotationId_OrderDate",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesDeliveries_SalesOrderId_DeliveryDate",
                table: "SalesDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_PurchaseReceivingId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_PurchaseOrderId_ReceiptDate",
                table: "PurchaseReceivings");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_SupplierId",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "PurchaseReceivingId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "PurchaseReceivings");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_SupplierId_ReceiptDate",
                table: "PurchaseReceivings",
                columns: new[] { "SupplierId", "ReceiptDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_SupplierId_ReceiptDate",
                table: "PurchaseReceivings");

            migrationBuilder.AddColumn<int>(
                name: "SalesDeliveryId",
                table: "SalesReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderId",
                table: "SalesDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseReceivingId",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderId",
                table: "PurchaseReceivings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesDeliveryId_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "SalesDeliveryId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_QuotationId_OrderDate",
                table: "SalesOrders",
                columns: new[] { "QuotationId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_SalesOrderId_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "SalesOrderId", "DeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseReceivingId",
                table: "PurchaseReturns",
                column: "PurchaseReceivingId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_PurchaseOrderId_ReceiptDate",
                table: "PurchaseReceivings",
                columns: new[] { "PurchaseOrderId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_SupplierId",
                table: "PurchaseReceivings",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReceivings",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseReceivings_PurchaseReceivingId",
                table: "PurchaseReturns",
                column: "PurchaseReceivingId",
                principalTable: "PurchaseReceivings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_SalesOrders_SalesOrderId",
                table: "SalesDeliveries",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Quotations_QuotationId",
                table: "SalesOrders",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                table: "SalesReturns",
                column: "SalesDeliveryId",
                principalTable: "SalesDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
