using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RefactorScaleRecord_AddWeightFields_RemoveScaleTypeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScaleRecords_ScaleTypes_ScaleTypeId",
                table: "ScaleRecords");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "EmployeeBankAccounts");

            migrationBuilder.RenameColumn(
                name: "TotalWeight",
                table: "ScaleRecords",
                newName: "NetWeight");

            migrationBuilder.RenameColumn(
                name: "ScaleTypeId",
                table: "ScaleRecords",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ScaleRecords",
                newName: "ExitWeight");

            migrationBuilder.RenameIndex(
                name: "IX_ScaleRecords_ScaleTypeId",
                table: "ScaleRecords",
                newName: "IX_ScaleRecords_ProductId");

            // 清空舊 ScaleTypeId 資料（原值為 ScaleType PK，與 Product PK 不對應，避免 FK 違反）
            migrationBuilder.Sql("UPDATE [ScaleRecords] SET [ProductId] = NULL");

            migrationBuilder.AddColumn<DateTime>(
                name: "EntryTime",
                table: "ScaleRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EntryWeight",
                table: "ScaleRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitTime",
                table: "ScaleRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "EmployeeBankAccounts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ScaleRecords_Products_ProductId",
                table: "ScaleRecords",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ScaleRecords_Products_ProductId",
                table: "ScaleRecords");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeBankAccounts_BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "EntryTime",
                table: "ScaleRecords");

            migrationBuilder.DropColumn(
                name: "EntryWeight",
                table: "ScaleRecords");

            migrationBuilder.DropColumn(
                name: "ExitTime",
                table: "ScaleRecords");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "EmployeeBankAccounts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ScaleRecords",
                newName: "ScaleTypeId");

            migrationBuilder.RenameColumn(
                name: "NetWeight",
                table: "ScaleRecords",
                newName: "TotalWeight");

            migrationBuilder.RenameColumn(
                name: "ExitWeight",
                table: "ScaleRecords",
                newName: "Quantity");

            migrationBuilder.RenameIndex(
                name: "IX_ScaleRecords_ProductId",
                table: "ScaleRecords",
                newName: "IX_ScaleRecords_ScaleTypeId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ScaleRecords_ScaleTypes_ScaleTypeId",
                table: "ScaleRecords",
                column: "ScaleTypeId",
                principalTable: "ScaleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
