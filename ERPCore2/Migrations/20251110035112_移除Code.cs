using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 移除Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_SalesReturnNumber",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_SalesOrderNumber",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesDeliveries_DeliveryNumber",
                table: "SalesDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_QuotationNumber",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_PurchaseReturnNumber",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_ReceiptNumber",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "SalesReturnNumber",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "SalesOrderNumber",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryNumber",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "QuotationNumber",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "PurchaseReturnNumber",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "PurchaseReceivings");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_Code",
                table: "SalesReturns",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_Code",
                table: "SalesOrders",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_Code",
                table: "SalesDeliveries",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_Code",
                table: "Quotations",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_Code",
                table: "PurchaseReturns",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_Code",
                table: "PurchaseReceivings",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_Code",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_Code",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesDeliveries_Code",
                table: "SalesDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_Code",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_Code",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_Code",
                table: "PurchaseReceivings");

            migrationBuilder.AddColumn<string>(
                name: "SalesReturnNumber",
                table: "SalesReturns",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalesOrderNumber",
                table: "SalesOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNumber",
                table: "SalesDeliveries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotationNumber",
                table: "Quotations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseReturnNumber",
                table: "PurchaseReturns",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "PurchaseReceivings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesReturnNumber",
                table: "SalesReturns",
                column: "SalesReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SalesOrderNumber",
                table: "SalesOrders",
                column: "SalesOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_DeliveryNumber",
                table: "SalesDeliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationNumber",
                table: "Quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseReturnNumber",
                table: "PurchaseReturns",
                column: "PurchaseReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_ReceiptNumber",
                table: "PurchaseReceivings",
                column: "ReceiptNumber",
                unique: true);
        }
    }
}
