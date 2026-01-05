using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class ChangeShowBomOnPrintToNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ShowBomOnPrint",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ShowBomOnPrint",
                table: "Products",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
