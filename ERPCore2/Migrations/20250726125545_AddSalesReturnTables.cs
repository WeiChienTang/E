using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReturnTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesReturnNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedProcessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualProcessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnStatus = table.Column<int>(type: "int", nullable: false),
                    ReturnReason = table.Column<int>(type: "int", nullable: false),
                    ProcessPersonnel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReturnDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalReturnAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturnAmountWithTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRefunded = table.Column<bool>(type: "bit", nullable: false),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    SalesDeliveryId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                        column: x => x.SalesDeliveryId,
                        principalTable: "SalesDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturns_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OriginalUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProcessedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PendingQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsRestocked = table.Column<bool>(type: "bit", nullable: false),
                    RestockedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ScrapQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DetailRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QualityCondition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SalesReturnId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    SalesDeliveryDetailId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesDeliveryDetails_SalesDeliveryDetailId",
                        column: x => x.SalesDeliveryDetailId,
                        principalTable: "SalesDeliveryDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesReturns_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalTable: "SalesReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ProductId",
                table: "SalesReturnDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                column: "SalesDeliveryDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesOrderDetailId",
                table: "SalesReturnDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesReturnId_ProductId",
                table: "SalesReturnDetails",
                columns: new[] { "SalesReturnId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CustomerId_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "CustomerId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_EmployeeId",
                table: "SalesReturns",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ReturnStatus_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "ReturnStatus", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesDeliveryId",
                table: "SalesReturns",
                column: "SalesDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesOrderId_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "SalesOrderId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesReturnNumber",
                table: "SalesReturns",
                column: "SalesReturnNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesReturnDetails");

            migrationBuilder.DropTable(
                name: "SalesReturns");
        }
    }
}
