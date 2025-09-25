using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 財物的屬性 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailRemarks",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "DetailRemarks",
                table: "SalesOrderDetails");

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "SalesReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "SalesReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaidAmount",
                table: "SalesReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "SalesOrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedAmount",
                table: "SalesOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalReceivedAmount",
                table: "SalesOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "PurchaseReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedAmount",
                table: "PurchaseReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalReceivedAmount",
                table: "PurchaseReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "PurchaseReceivingDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "PurchaseReceivingDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaidAmount",
                table: "PurchaseReceivingDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "TotalPaidAmount",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "ReceivedAmount",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "TotalReceivedAmount",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "ReceivedAmount",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "TotalReceivedAmount",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "TotalPaidAmount",
                table: "PurchaseReceivingDetails");

            migrationBuilder.AddColumn<string>(
                name: "DetailRemarks",
                table: "SalesReturnDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailRemarks",
                table: "SalesOrderDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
