using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminUserSystemFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 將 admin 使用者設置為系統使用者
            migrationBuilder.Sql("UPDATE Employees SET IsSystemUser = 1 WHERE Account = 'admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 回復 admin 使用者的系統使用者標記
            migrationBuilder.Sql("UPDATE Employees SET IsSystemUser = 0 WHERE Account = 'admin'");
        }
    }
}
