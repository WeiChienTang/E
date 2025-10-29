using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableQuotationApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Quotations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Quotations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Quotations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_ApprovedBy",
                table: "Quotations",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Employees_ApprovedBy",
                table: "Quotations",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Employees_ApprovedBy",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_ApprovedBy",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "EnableQuotationApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Quotations");
        }
    }
}
