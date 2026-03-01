using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFieldsToRemainingModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableSalesDeliveryApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "SalesReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "SalesReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "SalesReturns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "SalesOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "SalesOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "PurchaseReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "PurchaseReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "PurchaseReturns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PurchaseReceivings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "PurchaseReceivings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "PurchaseReceivings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "PurchaseReceivings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ApprovedBy",
                table: "SalesReturns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ApprovedBy",
                table: "SalesOrders",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ApprovedBy",
                table: "PurchaseReturns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_ApprovedBy",
                table: "PurchaseReceivings",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Employees_ApprovedBy",
                table: "PurchaseReceivings",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Employees_ApprovedBy",
                table: "PurchaseReturns",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Employees_ApprovedBy",
                table: "SalesOrders",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Employees_ApprovedBy",
                table: "SalesReturns",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Employees_ApprovedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Employees_ApprovedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Employees_ApprovedBy",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Employees_ApprovedBy",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_ApprovedBy",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_ApprovedBy",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_ApprovedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_ApprovedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "EnableSalesDeliveryApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "PurchaseReceivings");
        }
    }
}
