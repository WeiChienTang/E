using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountsPayableSetoffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SetoffDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TotalSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    PaymentAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AccountsPayableSetoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffs_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffs_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountsPayableSetoffDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffId = table.Column<int>(type: "int", nullable: false),
                    PurchaseReceivingDetailId = table.Column<int>(type: "int", nullable: true),
                    PurchaseReturnDetailId = table.Column<int>(type: "int", nullable: true),
                    PayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SetoffAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AfterPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_AccountsPayableSetoffDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffDetails_AccountsPayableSetoffs_SetoffId",
                        column: x => x.SetoffId,
                        principalTable: "AccountsPayableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffDetails_PurchaseReceivingDetails_PurchaseReceivingDetailId",
                        column: x => x.PurchaseReceivingDetailId,
                        principalTable: "PurchaseReceivingDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffDetails_PurchaseReturnDetails_PurchaseReturnDetailId",
                        column: x => x.PurchaseReturnDetailId,
                        principalTable: "PurchaseReturnDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountsPayableSetoffPaymentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AccountsPayableSetoffPaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffPaymentDetails_AccountsPayableSetoffs_SetoffId",
                        column: x => x.SetoffId,
                        principalTable: "AccountsPayableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffPaymentDetails_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountsPayableSetoffPaymentDetails_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffDetails_PurchaseReceivingDetailId",
                table: "AccountsPayableSetoffDetails",
                column: "PurchaseReceivingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffDetails_PurchaseReturnDetailId",
                table: "AccountsPayableSetoffDetails",
                column: "PurchaseReturnDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffDetails_SetoffId",
                table: "AccountsPayableSetoffDetails",
                column: "SetoffId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffPaymentDetails_BankId",
                table: "AccountsPayableSetoffPaymentDetails",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffPaymentDetails_PaymentMethodId",
                table: "AccountsPayableSetoffPaymentDetails",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffPaymentDetails_SetoffId",
                table: "AccountsPayableSetoffPaymentDetails",
                column: "SetoffId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffs_CompanyId",
                table: "AccountsPayableSetoffs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffs_PaymentMethodId",
                table: "AccountsPayableSetoffs",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffs_SetoffDate",
                table: "AccountsPayableSetoffs",
                column: "SetoffDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffs_SetoffNumber",
                table: "AccountsPayableSetoffs",
                column: "SetoffNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayableSetoffs_SupplierId",
                table: "AccountsPayableSetoffs",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountsPayableSetoffDetails");

            migrationBuilder.DropTable(
                name: "AccountsPayableSetoffPaymentDetails");

            migrationBuilder.DropTable(
                name: "AccountsPayableSetoffs");
        }
    }
}
