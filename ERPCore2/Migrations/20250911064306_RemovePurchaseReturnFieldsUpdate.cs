using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemovePurchaseReturnFieldsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Employees_ConfirmedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Employees_EmployeeId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Warehouses_WarehouseId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_ConfirmedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_EmployeeId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_PurchaseOrderId_ReturnDate",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_WarehouseId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ActualProcessDate",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ExpectedProcessDate",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ProcessPersonnel",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PurchaseReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualProcessDate",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedBy",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedProcessDate",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessPersonnel",
                table: "PurchaseReturns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderId",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "PurchaseReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

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
                name: "IX_PurchaseReturns_WarehouseId",
                table: "PurchaseReturns",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Employees_ConfirmedBy",
                table: "PurchaseReturns",
                column: "ConfirmedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Employees_EmployeeId",
                table: "PurchaseReturns",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Warehouses_WarehouseId",
                table: "PurchaseReturns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
