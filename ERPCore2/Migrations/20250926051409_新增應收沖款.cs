using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增應收沖款 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountsReceivableSetoffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SetoffDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TotalSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    PaymentAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApproverId = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccountsReceivableSetoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffs_Employees_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffs_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountsReceivableSetoffDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    SalesReturnDetailId = table.Column<int>(type: "int", nullable: true),
                    ReceivableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SetoffAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AfterReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsFullyReceived = table.Column<bool>(type: "bit", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_AccountsReceivableSetoffDetails", x => x.Id);
                    table.CheckConstraint("CK_AccountsReceivableSetoffDetail_RelatedDetail", "SalesOrderDetailId IS NOT NULL OR SalesReturnDetailId IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffDetails_AccountsReceivableSetoffs_SetoffId",
                        column: x => x.SetoffId,
                        principalTable: "AccountsReceivableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountsReceivableSetoffDetails_SalesReturnDetails_SalesReturnDetailId",
                        column: x => x.SalesReturnDetailId,
                        principalTable: "SalesReturnDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffDetails_SalesOrderDetailId",
                table: "AccountsReceivableSetoffDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffDetails_SalesReturnDetailId",
                table: "AccountsReceivableSetoffDetails",
                column: "SalesReturnDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffDetails_SetoffId",
                table: "AccountsReceivableSetoffDetails",
                column: "SetoffId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_ApproverId",
                table: "AccountsReceivableSetoffs",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_CustomerId",
                table: "AccountsReceivableSetoffs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_PaymentMethodId",
                table: "AccountsReceivableSetoffs",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_SetoffDate",
                table: "AccountsReceivableSetoffs",
                column: "SetoffDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_SetoffNumber",
                table: "AccountsReceivableSetoffs",
                column: "SetoffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "AccountsReceivableSetoffs");
        }
    }
}
