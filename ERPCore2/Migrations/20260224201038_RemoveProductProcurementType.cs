using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductProcurementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProcurementType",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ListPrice",
                table: "Products",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcurementType",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
