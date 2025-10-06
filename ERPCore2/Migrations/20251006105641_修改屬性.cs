using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 修改屬性 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsRestocked",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "PendingQuantity",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "ProcessedQuantity",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "QualityCondition",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "RestockedQuantity",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "ReturnUnitPrice",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "ScrapQuantity",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "DeliveredQuantity",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "PendingQuantity",
                table: "SalesOrderDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SalesReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "SalesReturnDetails",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestocked",
                table: "SalesReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingQuantity",
                table: "SalesReturnDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProcessedQuantity",
                table: "SalesReturnDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "QualityCondition",
                table: "SalesReturnDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RestockedQuantity",
                table: "SalesReturnDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnUnitPrice",
                table: "SalesReturnDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ScrapQuantity",
                table: "SalesReturnDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveredQuantity",
                table: "SalesOrderDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SalesOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingQuantity",
                table: "SalesOrderDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
