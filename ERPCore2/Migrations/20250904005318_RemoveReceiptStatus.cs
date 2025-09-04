using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReceiptStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_ReceiptStatus_ReceiptDate",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "ReceiptStatus",
                table: "PurchaseReceivings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptStatus",
                table: "PurchaseReceivings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_ReceiptStatus_ReceiptDate",
                table: "PurchaseReceivings",
                columns: new[] { "ReceiptStatus", "ReceiptDate" });
        }
    }
}
