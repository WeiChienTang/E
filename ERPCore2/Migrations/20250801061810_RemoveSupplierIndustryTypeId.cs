using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSupplierIndustryTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_IndustryTypes_IndustryTypeId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_IndustryTypeId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IndustryTypeId",
                table: "Suppliers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndustryTypeId",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IndustryTypeId",
                table: "Suppliers",
                column: "IndustryTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_IndustryTypes_IndustryTypeId",
                table: "Suppliers",
                column: "IndustryTypeId",
                principalTable: "IndustryTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
