using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增SetoffPayment和SetoffProductDetail資料 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prepayments");

            migrationBuilder.CreateTable(
                name: "SetoffPrepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrepaymentType = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: true),
                    FinancialTransactionId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SetoffPrepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_Code",
                table: "SetoffPrepayments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_CustomerId",
                table: "SetoffPrepayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_FinancialTransactionId",
                table: "SetoffPrepayments",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_PrepaymentType",
                table: "SetoffPrepayments",
                column: "PrepaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SetoffDocumentId",
                table: "SetoffPrepayments",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments",
                column: "SourceDocumentCode");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SupplierId",
                table: "SetoffPrepayments",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetoffPrepayments");

            migrationBuilder.CreateTable(
                name: "Prepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    FinancialTransactionId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrepaymentType = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prepayments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prepayments_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prepayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
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
                name: "IX_Prepayments_CustomerId",
                table: "Prepayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_FinancialTransactionId",
                table: "Prepayments",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_PaymentDate",
                table: "Prepayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_PrepaymentType",
                table: "Prepayments",
                column: "PrepaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_SupplierId",
                table: "Prepayments",
                column: "SupplierId");
        }
    }
}
