using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddIsJournalizedToSourceDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "SalesReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "SalesDeliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "SalesDeliveries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "PurchaseReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "PurchaseReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsJournalized",
                table: "PurchaseReceivings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "JournalizedAt",
                table: "PurchaseReceivings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "IsJournalized",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "JournalizedAt",
                table: "PurchaseReceivings");
        }
    }
}
