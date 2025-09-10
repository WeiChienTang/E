using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSalesOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_OrderStatus_OrderDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ActualDeliveryDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "OrderRemarks",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "OrderStatus",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SalesPersonnel",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SalesType",
                table: "SalesOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDeliveryDate",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderRemarks",
                table: "SalesOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderStatus",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SalesPersonnel",
                table: "SalesOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesType",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_OrderStatus_OrderDate",
                table: "SalesOrders",
                columns: new[] { "OrderStatus", "OrderDate" });
        }
    }
}
