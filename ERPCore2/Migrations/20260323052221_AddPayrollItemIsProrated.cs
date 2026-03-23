using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollItemIsProrated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId1",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeBankAccounts_BankId1",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankId1",
                table: "EmployeeBankAccounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsProrated",
                table: "PayrollItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProrated",
                table: "PayrollItems");

            migrationBuilder.AddColumn<int>(
                name: "BankId1",
                table: "EmployeeBankAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBankAccounts_BankId1",
                table: "EmployeeBankAccounts",
                column: "BankId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId1",
                table: "EmployeeBankAccounts",
                column: "BankId1",
                principalTable: "Banks",
                principalColumn: "Id");
        }
    }
}
