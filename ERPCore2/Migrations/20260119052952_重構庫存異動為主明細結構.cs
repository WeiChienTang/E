using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 重構庫存異動為主明細結構 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_WarehouseLocations_WarehouseLocationId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_ProductId_TransactionDate",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_WarehouseLocationId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionBatchDate",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionExpiryDate",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "InventoryTransactions");

            migrationBuilder.RenameColumn(
                name: "WarehouseLocationId",
                table: "InventoryTransactions",
                newName: "SourceDocumentId");

            migrationBuilder.RenameColumn(
                name: "TransactionBatchNumber",
                table: "InventoryTransactions",
                newName: "SourceDocumentType");

            migrationBuilder.RenameColumn(
                name: "StockBefore",
                table: "InventoryTransactions",
                newName: "TotalQuantity");

            migrationBuilder.RenameColumn(
                name: "StockAfter",
                table: "InventoryTransactions",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "InventoryStockDetailId",
                table: "InventoryTransactions",
                newName: "EmployeeId");

            // 清空 EmployeeId 的舊值（原為 InventoryStockDetailId）
            migrationBuilder.Sql("UPDATE [InventoryTransactions] SET [EmployeeId] = NULL");
            
            // 清空重命名後的欄位值（原有值不適用於新用途）
            migrationBuilder.Sql("UPDATE [InventoryTransactions] SET [SourceDocumentId] = NULL");
            migrationBuilder.Sql("UPDATE [InventoryTransactions] SET [SourceDocumentType] = NULL");
            migrationBuilder.Sql("UPDATE [InventoryTransactions] SET [TotalQuantity] = 0");
            migrationBuilder.Sql("UPDATE [InventoryTransactions] SET [TotalAmount] = 0");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_InventoryStockDetailId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_EmployeeId");

            migrationBuilder.CreateTable(
                name: "InventoryTransactionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryTransactionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockBefore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockAfter = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockDetailId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryTransactionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryStockDetails_InventoryStockDetailId",
                        column: x => x.InventoryStockDetailId,
                        principalTable: "InventoryStockDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_SourceDocumentType_SourceDocumentId",
                table: "InventoryTransactions",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                table: "InventoryTransactions",
                column: "TransactionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryStockDetailId",
                table: "InventoryTransactionDetails",
                column: "InventoryStockDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryStockId",
                table: "InventoryTransactionDetails",
                column: "InventoryStockId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryTransactionId",
                table: "InventoryTransactionDetails",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_ProductId",
                table: "InventoryTransactionDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_WarehouseLocationId",
                table: "InventoryTransactionDetails",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Employees_EmployeeId",
                table: "InventoryTransactions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                table: "InventoryTransactions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Employees_EmployeeId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                table: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InventoryTransactionDetails");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_SourceDocumentType_SourceDocumentId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                table: "InventoryTransactions");

            migrationBuilder.RenameColumn(
                name: "TotalQuantity",
                table: "InventoryTransactions",
                newName: "StockBefore");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "InventoryTransactions",
                newName: "StockAfter");

            migrationBuilder.RenameColumn(
                name: "SourceDocumentType",
                table: "InventoryTransactions",
                newName: "TransactionBatchNumber");

            migrationBuilder.RenameColumn(
                name: "SourceDocumentId",
                table: "InventoryTransactions",
                newName: "WarehouseLocationId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "InventoryTransactions",
                newName: "InventoryStockDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_EmployeeId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_InventoryStockDetailId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "InventoryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "InventoryTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionBatchDate",
                table: "InventoryTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionExpiryDate",
                table: "InventoryTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "InventoryTransactions",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_WarehouseLocationId",
                table: "InventoryTransactions",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryTransactions",
                column: "InventoryStockDetailId",
                principalTable: "InventoryStockDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_WarehouseLocations_WarehouseLocationId",
                table: "InventoryTransactions",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                table: "InventoryTransactions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
