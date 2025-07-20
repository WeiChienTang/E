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
            // 移除舊的索引
            migrationBuilder.DropIndex(
                name: "IX_Employees_Username",
                table: "Employees");

            // 重命名欄位
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Employees",
                newName: "Account");

            // 創建新的索引
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
            // 移除新的索引
            migrationBuilder.DropIndex(
                name: "IX_Employees_Account",
                table: "Employees");

            // 恢復欄位名稱
            migrationBuilder.RenameColumn(
                name: "Account",
                table: "Employees",
                newName: "Username");

            // 恢復舊的索引
            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }
    }
}
