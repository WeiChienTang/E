using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增銷貨出貨單與調整銷售流程 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesDeliveryId",
                table: "SalesReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveredQuantity",
                table: "SalesOrderDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderId",
                table: "ProductionSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsShipped = table.Column<bool>(type: "bit", nullable: false),
                    ActualShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesDeliveryId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesDeliveryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_SalesDeliveries_SalesDeliveryId",
                        column: x => x.SalesDeliveryId,
                        principalTable: "SalesDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_SalesDeliveryId",
                table: "SalesReturns",
                column: "SalesDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                column: "SalesDeliveryDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_SalesOrderId",
                table: "ProductionSchedules",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_ApprovedBy",
                table: "SalesDeliveries",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_CustomerId_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "CustomerId", "DeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_DeliveryNumber",
                table: "SalesDeliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_EmployeeId",
                table: "SalesDeliveries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_SalesOrderId_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "SalesOrderId", "DeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_WarehouseId",
                table: "SalesDeliveries",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_ProductId",
                table: "SalesDeliveryDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_SalesDeliveryId_ProductId",
                table: "SalesDeliveryDetails",
                columns: new[] { "SalesDeliveryId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_SalesOrderDetailId",
                table: "SalesDeliveryDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_UnitId",
                table: "SalesDeliveryDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_WarehouseId",
                table: "SalesDeliveryDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_WarehouseLocationId",
                table: "SalesDeliveryDetails",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionSchedules_SalesOrders_SalesOrderId",
                table: "ProductionSchedules",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnDetails_SalesDeliveryDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                column: "SalesDeliveryDetailId",
                principalTable: "SalesDeliveryDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                table: "SalesReturns",
                column: "SalesDeliveryId",
                principalTable: "SalesDeliveries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionSchedules_SalesOrders_SalesOrderId",
                table: "ProductionSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnDetails_SalesDeliveryDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropTable(
                name: "SalesDeliveryDetails");

            migrationBuilder.DropTable(
                name: "SalesDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProductionSchedules_SalesOrderId",
                table: "ProductionSchedules");

            migrationBuilder.DropColumn(
                name: "SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "SalesDeliveryDetailId",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "DeliveredQuantity",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "ProductionSchedules");
        }
    }
}
