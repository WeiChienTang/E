using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class SupplierPaymentMethodRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Suppliers");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PaymentMethodId",
                table: "Suppliers",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_PaymentMethods_PaymentMethodId",
                table: "Suppliers",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_PaymentMethods_PaymentMethodId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_PaymentMethodId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Suppliers");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
