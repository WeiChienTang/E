using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReturnReasonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnReason",
                table: "SalesReturns",
                newName: "ReturnReasonId");

            migrationBuilder.CreateTable(
                name: "SalesReturnReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnReasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ReturnReasonId",
                table: "SalesReturns",
                column: "ReturnReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnReasons_Name",
                table: "SalesReturnReasons",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesReturnReasons_ReturnReasonId",
                table: "SalesReturns",
                column: "ReturnReasonId",
                principalTable: "SalesReturnReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesReturnReasons_ReturnReasonId",
                table: "SalesReturns");

            migrationBuilder.DropTable(
                name: "SalesReturnReasons");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_ReturnReasonId",
                table: "SalesReturns");

            migrationBuilder.RenameColumn(
                name: "ReturnReasonId",
                table: "SalesReturns",
                newName: "ReturnReason");
        }
    }
}
