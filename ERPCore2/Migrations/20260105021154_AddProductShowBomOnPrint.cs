using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddProductShowBomOnPrint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowBomOnPrint",
                table: "Products",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowBomOnPrint",
                table: "Products");
        }
    }
}
