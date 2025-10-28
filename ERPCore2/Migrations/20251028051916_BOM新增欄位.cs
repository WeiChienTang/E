using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class BOM新增欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProductCompositions");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "ProductCompositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ProductCompositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specification",
                table: "ProductCompositions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_CreatedByEmployeeId",
                table: "ProductCompositions",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_CustomerId",
                table: "ProductCompositions",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCompositions_Customers_CustomerId",
                table: "ProductCompositions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCompositions_Employees_CreatedByEmployeeId",
                table: "ProductCompositions",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCompositions_Customers_CustomerId",
                table: "ProductCompositions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCompositions_Employees_CreatedByEmployeeId",
                table: "ProductCompositions");

            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_CreatedByEmployeeId",
                table: "ProductCompositions");

            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_CustomerId",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "Specification",
                table: "ProductCompositions");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProductCompositions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
