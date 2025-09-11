using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemovePurchaseReceivingConfirmedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceivings_Employees_ConfirmedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceivings_ConfirmedBy",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                table: "PurchaseReceivings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "PurchaseReceivings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedBy",
                table: "PurchaseReceivings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_ConfirmedBy",
                table: "PurchaseReceivings",
                column: "ConfirmedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceivings_Employees_ConfirmedBy",
                table: "PurchaseReceivings",
                column: "ConfirmedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
