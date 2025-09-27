using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountsReceivableSetoffs_Employees_ApproverId",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropIndex(
                name: "IX_AccountsReceivableSetoffs_ApproverId",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropColumn(
                name: "ApprovalRemarks",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropColumn(
                name: "ApproverId",
                table: "AccountsReceivableSetoffs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalRemarks",
                table: "AccountsReceivableSetoffs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "AccountsReceivableSetoffs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApproverId",
                table: "AccountsReceivableSetoffs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_ApproverId",
                table: "AccountsReceivableSetoffs",
                column: "ApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountsReceivableSetoffs_Employees_ApproverId",
                table: "AccountsReceivableSetoffs",
                column: "ApproverId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
