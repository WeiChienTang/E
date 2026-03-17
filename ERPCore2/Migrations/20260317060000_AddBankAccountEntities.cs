using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 移除客戶舊的銀行字串欄位（改為 CustomerBankAccounts 關聯表）
            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Customers");

            // 移除廠商舊的銀行字串欄位（改為 SupplierBankAccounts 關聯表）
            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Suppliers");

            // 建立客戶銀行帳號表
            migrationBuilder.CreateTable(
                name: "CustomerBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerBankAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 建立廠商銀行帳號表
            migrationBuilder.CreateTable(
                name: "SupplierBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierBankAccounts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 建立公司銀行帳號表
            migrationBuilder.CreateTable(
                name: "CompanyBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 建立索引
            migrationBuilder.CreateIndex(
                name: "IX_CustomerBankAccounts_BankId",
                table: "CustomerBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBankAccounts_CustomerId",
                table: "CustomerBankAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierBankAccounts_BankId",
                table: "SupplierBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierBankAccounts_SupplierId",
                table: "SupplierBankAccounts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankAccounts_BankId",
                table: "CompanyBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankAccounts_CompanyId",
                table: "CompanyBankAccounts",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomerBankAccounts");
            migrationBuilder.DropTable(name: "SupplierBankAccounts");
            migrationBuilder.DropTable(name: "CompanyBankAccounts");

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
