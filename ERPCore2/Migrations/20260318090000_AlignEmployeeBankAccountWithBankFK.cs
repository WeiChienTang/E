using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AlignEmployeeBankAccountWithBankFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 移除舊的字串欄位
            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "EmployeeBankAccounts");

            // 修改 AccountNumber 長度與 CustomerBankAccounts/SupplierBankAccounts 一致（20→30）
            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "EmployeeBankAccounts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // 新增 BankId FK
            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "EmployeeBankAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBankAccounts_BankId",
                table: "EmployeeBankAccounts",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId",
                table: "EmployeeBankAccounts",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeBankAccounts_BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "EmployeeBankAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "EmployeeBankAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "EmployeeBankAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
