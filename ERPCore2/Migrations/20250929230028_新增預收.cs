using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增預收 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustmentAmount",
                table: "FinancialTransactions");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "FinancialTransactions",
                newName: "CurrentDiscountAmount");

            migrationBuilder.RenameColumn(
                name: "CashAmount",
                table: "FinancialTransactions",
                newName: "AccumulatedDiscountAmount");

            migrationBuilder.CreateTable(
                name: "Prepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrepaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrepaymentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SetoffId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prepayments_AccountsReceivableSetoffs_SetoffId",
                        column: x => x.SetoffId,
                        principalTable: "AccountsReceivableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prepayments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_Code",
                table: "Prepayments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_CompanyId",
                table: "Prepayments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_PrepaymentDate",
                table: "Prepayments",
                column: "PrepaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_SetoffId",
                table: "Prepayments",
                column: "SetoffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prepayments");

            migrationBuilder.RenameColumn(
                name: "CurrentDiscountAmount",
                table: "FinancialTransactions",
                newName: "DiscountAmount");

            migrationBuilder.RenameColumn(
                name: "AccumulatedDiscountAmount",
                table: "FinancialTransactions",
                newName: "CashAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustmentAmount",
                table: "FinancialTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
