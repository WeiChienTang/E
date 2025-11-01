using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增屬性d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyAddress",
                table: "Suppliers",
                newName: "SupplierAddress");

            migrationBuilder.AddColumn<string>(
                name: "SupplierContactPhone",
                table: "Suppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierContactPhone",
                table: "Suppliers");

            migrationBuilder.RenameColumn(
                name: "SupplierAddress",
                table: "Suppliers",
                newName: "CompanyAddress");
        }
    }
}
