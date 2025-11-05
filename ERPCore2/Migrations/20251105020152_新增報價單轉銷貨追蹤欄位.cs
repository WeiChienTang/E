using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增報價單轉銷貨追蹤欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotationDetailId",
                table: "SalesOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedQuantity",
                table: "QuotationDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_QuotationDetailId",
                table: "SalesOrderDetails",
                column: "QuotationDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetails_QuotationDetails_QuotationDetailId",
                table: "SalesOrderDetails",
                column: "QuotationDetailId",
                principalTable: "QuotationDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetails_QuotationDetails_QuotationDetailId",
                table: "SalesOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetails_QuotationDetailId",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "QuotationDetailId",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "ConvertedQuantity",
                table: "QuotationDetails");
        }
    }
}
