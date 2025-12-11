using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalReturnedQuantityToPurchaseReceivingDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 新增欄位
            migrationBuilder.AddColumn<int>(
                name: "TotalReturnedQuantity",
                table: "PurchaseReceivingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            // 2. 回填現有資料的退貨數量
            migrationBuilder.Sql(@"
                UPDATE PurchaseReceivingDetails
                SET TotalReturnedQuantity = (
                    SELECT ISNULL(SUM(ReturnQuantity), 0)
                    FROM PurchaseReturnDetails
                    WHERE PurchaseReturnDetails.PurchaseReceivingDetailId = PurchaseReceivingDetails.Id
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalReturnedQuantity",
                table: "PurchaseReceivingDetails");
        }
    }
}
