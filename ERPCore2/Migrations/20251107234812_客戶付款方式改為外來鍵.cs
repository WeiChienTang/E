using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 客戶付款方式改為外來鍵 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PaymentMethodId",
                table: "Customers",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_PaymentMethods_PaymentMethodId",
                table: "Customers",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PaymentMethods_PaymentMethodId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PaymentMethodId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
