using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalReturnedQuantityToSalesDeliveryDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 新增欄位
            migrationBuilder.AddColumn<decimal>(
                name: "TotalReturnedQuantity",
                table: "SalesDeliveryDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
            
            // 2. 回填現有資料的退貨數量
            migrationBuilder.Sql(@"
                UPDATE SalesDeliveryDetails
                SET TotalReturnedQuantity = (
                    SELECT ISNULL(SUM(ReturnQuantity), 0)
                    FROM SalesReturnDetails
                    WHERE SalesReturnDetails.SalesDeliveryDetailId = SalesDeliveryDetails.Id
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalReturnedQuantity",
                table: "SalesDeliveryDetails");
        }
    }
}
