using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFinancialTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepayments_FinancialTransactions_FinancialTransactionId",
                table: "SetoffPrepayments");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SetoffPrepayments_FinancialTransactionId",
                table: "SetoffPrepayments");

            migrationBuilder.DropColumn(
                name: "FinancialTransactionId",
                table: "SetoffPrepayments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinancialTransactionId",
                table: "SetoffPrepayments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    ReversalTransactionId = table.Column<int>(type: "int", nullable: true),
                    AccumulatedDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CurrentDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PaymentAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReversedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: true),
                    SourceDetailId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialTransactions_ReversalTransactionId",
                        column: x => x.ReversalTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_FinancialTransactionId",
                table: "SetoffPrepayments",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BankId",
                table: "FinancialTransactions",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CompanyId_TransactionDate",
                table: "FinancialTransactions",
                columns: new[] { "CompanyId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CustomerId_TransactionDate",
                table: "FinancialTransactions",
                columns: new[] { "CustomerId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_PaymentMethodId",
                table: "FinancialTransactions",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ReversalTransactionId",
                table: "FinancialTransactions",
                column: "ReversalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SetoffDocumentId",
                table: "FinancialTransactions",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SourceDocumentType_SourceDocumentId",
                table: "FinancialTransactions",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionNumber",
                table: "FinancialTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionType_TransactionDate",
                table: "FinancialTransactions",
                columns: new[] { "TransactionType", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_VendorId",
                table: "FinancialTransactions",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_VendorId_TransactionDate",
                table: "FinancialTransactions",
                columns: new[] { "VendorId", "TransactionDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepayments_FinancialTransactions_FinancialTransactionId",
                table: "SetoffPrepayments",
                column: "FinancialTransactionId",
                principalTable: "FinancialTransactions",
                principalColumn: "Id");
        }
    }
}
