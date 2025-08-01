using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColorFieldsHexCodeAndRgbValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlueValue",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "GreenValue",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "HexCode",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "RedValue",
                table: "Colors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlueValue",
                table: "Colors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GreenValue",
                table: "Colors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HexCode",
                table: "Colors",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RedValue",
                table: "Colors",
                type: "int",
                nullable: true);
        }
    }
}
