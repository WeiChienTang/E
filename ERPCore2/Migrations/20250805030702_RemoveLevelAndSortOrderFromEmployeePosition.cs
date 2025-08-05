using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLevelAndSortOrderFromEmployeePosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "EmployeePositions");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "EmployeePositions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "EmployeePositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "EmployeePositions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
