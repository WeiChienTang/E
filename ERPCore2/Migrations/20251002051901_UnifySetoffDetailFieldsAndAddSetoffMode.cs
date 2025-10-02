using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UnifySetoffDetailFieldsAndAddSetoffMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnSubtotal",
                table: "SalesReturnDetails",
                newName: "ReturnSubtotalAmount");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "SalesOrderDetails",
                newName: "SubtotalAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnSubtotalAmount",
                table: "SalesReturnDetails",
                newName: "ReturnSubtotal");

            migrationBuilder.RenameColumn(
                name: "SubtotalAmount",
                table: "SalesOrderDetails",
                newName: "Subtotal");
        }
    }
}
