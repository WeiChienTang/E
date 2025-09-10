using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RrmoveSalesDeliverAndSalesDeliverDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "SalesDeliveryId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "SalesDeliveryDetailId",
                table: "SalesReturnDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "SalesDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    ActualArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryContact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeliveryPersonnel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeliveryRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    ExpectedArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SalesDeliveryId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DetailRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_SalesDeliveries_DeliveryNumber",
                table: "SalesDeliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_DeliveryStatus_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "DeliveryStatus", "DeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_EmployeeId",
                table: "SalesDeliveries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_SalesOrderId_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "SalesOrderId", "DeliveryDate" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnDetails_SalesDeliveryDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                column: "SalesDeliveryDetailId",
                principalTable: "SalesDeliveryDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesDeliveries_SalesDeliveryId",
                table: "SalesReturns",
                column: "SalesDeliveryId",
                principalTable: "SalesDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
