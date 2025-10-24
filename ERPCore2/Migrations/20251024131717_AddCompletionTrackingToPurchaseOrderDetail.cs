using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionTrackingToPurchaseOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "PurchaseOrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedByEmployeeId",
                table: "PurchaseOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_CompletedByEmployeeId",
                table: "PurchaseOrderDetails",
                column: "CompletedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderDetails_Employees_CompletedByEmployeeId",
                table: "PurchaseOrderDetails",
                column: "CompletedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderDetails_Employees_CompletedByEmployeeId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderDetails_CompletedByEmployeeId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropColumn(
                name: "CompletedByEmployeeId",
                table: "PurchaseOrderDetails");
        }
    }
}
