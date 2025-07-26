using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseReturnTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseReturnNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    PurchaseReceivingId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Employees_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_PurchaseReceivings_PurchaseReceivingId",
                        column: x => x.PurchaseReceivingId,
                        principalTable: "PurchaseReceivings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false),
                    OriginalUnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnUnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ProcessedQuantity = table.Column<int>(type: "int", nullable: false),
                    IsShipped = table.Column<bool>(type: "bit", nullable: false),
                    ShippedQuantity = table.Column<int>(type: "int", nullable: false),
                    ScrapQuantity = table.Column<int>(type: "int", nullable: false),
                    DetailRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QualityCondition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    PurchaseReceivingDetailId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturnDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseReceivingDetails_PurchaseReceivingDetailId",
                        column: x => x.PurchaseReceivingDetailId,
                        principalTable: "PurchaseReceivingDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseReturns_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_ProductId",
                table: "PurchaseReturnDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseOrderDetailId",
                table: "PurchaseReturnDetails",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseReceivingDetailId",
                table: "PurchaseReturnDetails",
                column: "PurchaseReceivingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseReturnId_ProductId",
                table: "PurchaseReturnDetails",
                columns: new[] { "PurchaseReturnId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_UnitId",
                table: "PurchaseReturnDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_WarehouseLocationId",
                table: "PurchaseReturnDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ConfirmedBy",
                table: "PurchaseReturns",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_EmployeeId",
                table: "PurchaseReturns",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseOrderId_ReturnDate",
                table: "PurchaseReturns",
                columns: new[] { "PurchaseOrderId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseReceivingId",
                table: "PurchaseReturns",
                column: "PurchaseReceivingId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseReturnNumber",
                table: "PurchaseReturns",
                column: "PurchaseReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ReturnStatus_ReturnDate",
                table: "PurchaseReturns",
                columns: new[] { "ReturnStatus", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_SupplierId_ReturnDate",
                table: "PurchaseReturns",
                columns: new[] { "SupplierId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_WarehouseId",
                table: "PurchaseReturns",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseReturnDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturns");
        }
    }
}
