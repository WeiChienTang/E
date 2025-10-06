using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReceivedAmountFromPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalReturnAmountWithTax",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReceivedAmount",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "TaxAmount",
                table: "PurchaseOrders",
                newName: "PurchaseTaxAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseTaxAmount",
                table: "PurchaseOrders",
                newName: "TaxAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalReturnAmountWithTax",
                table: "PurchaseReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedAmount",
                table: "PurchaseOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
