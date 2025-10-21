using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInventoryStockToMasterDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Products_ProductId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_WarehouseLocations_WarehouseLocationId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_WarehouseId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "BatchDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "InTransitStock",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "LastTransactionDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ReservedStock",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "InventoryStocks");

            migrationBuilder.RenameColumn(
                name: "WarehouseLocationId",
                table: "InventoryStocks",
                newName: "ProductId1");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_WarehouseLocationId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_ProductId1");

            migrationBuilder.AddColumn<int>(
                name: "InventoryStockDetailId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryStockDetailId",
                table: "InventoryReservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryStockDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    ReservedStock = table.Column<int>(type: "int", nullable: false),
                    InTransitStock = table.Column<int>(type: "int", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventoryStockDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryStockDetailId",
                table: "InventoryTransactions",
                column: "InventoryStockDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId",
                table: "InventoryStocks",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_InventoryStockDetailId",
                table: "InventoryReservations",
                column: "InventoryStockDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId",
                table: "InventoryStockDetails",
                columns: new[] { "InventoryStockId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_WarehouseId",
                table: "InventoryStockDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_WarehouseLocationId",
                table: "InventoryStockDetails",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReservations_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryReservations",
                column: "InventoryStockDetailId",
                principalTable: "InventoryStockDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Products_ProductId",
                table: "InventoryStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Products_ProductId1",
                table: "InventoryStocks",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryTransactions",
                column: "InventoryStockDetailId",
                principalTable: "InventoryStockDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReservations_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Products_ProductId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Products_ProductId1",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_InventoryStockDetails_InventoryStockDetailId",
                table: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InventoryStockDetails");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_InventoryStockDetailId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReservations_InventoryStockDetailId",
                table: "InventoryReservations");

            migrationBuilder.DropColumn(
                name: "InventoryStockDetailId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "InventoryStockDetailId",
                table: "InventoryReservations");

            migrationBuilder.RenameColumn(
                name: "ProductId1",
                table: "InventoryStocks",
                newName: "WarehouseLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_ProductId1",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_WarehouseLocationId");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "InventoryStocks",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BatchDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "InventoryStocks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "InventoryStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InTransitStock",
                table: "InventoryStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTransactionDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedStock",
                table: "InventoryStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "InventoryStocks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseId_WarehouseLocationId",
                table: "InventoryStocks",
                columns: new[] { "ProductId", "WarehouseId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL AND [WarehouseLocationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_WarehouseId",
                table: "InventoryStocks",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Products_ProductId",
                table: "InventoryStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_WarehouseLocations_WarehouseLocationId",
                table: "InventoryStocks",
                column: "WarehouseLocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Warehouses_WarehouseId",
                table: "InventoryStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }
    }
}
