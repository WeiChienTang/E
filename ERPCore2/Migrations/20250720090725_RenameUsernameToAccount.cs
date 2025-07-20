using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RenameUsernameToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Username",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Employees",
                newName: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Account",
                table: "Employees",
                column: "Account",
                unique: true,
                filter: "[Account] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Account",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Account",
                table: "Employees",
                newName: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }
    }
}
