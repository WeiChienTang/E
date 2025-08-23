using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsPrimarySupplierProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimarySupplier",
                table: "SupplierPricings");

            migrationBuilder.DropColumn(
                name: "IsPrimarySupplier",
                table: "ProductSuppliers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimarySupplier",
                table: "SupplierPricings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimarySupplier",
                table: "ProductSuppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
