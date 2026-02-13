using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardPanelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 步驟 1：建立新的面板表
            migrationBuilder.CreateTable(
                name: "EmployeeDashboardPanels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDashboardPanels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDashboardPanels_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardPanels_EmployeeId_SortOrder",
                table: "EmployeeDashboardPanels",
                columns: new[] { "EmployeeId", "SortOrder" });

            // 步驟 2：新增 PanelId 欄位（可為空）
            migrationBuilder.AddColumn<int>(
                name: "PanelId",
                table: "EmployeeDashboardConfigs",
                type: "int",
                nullable: true);

            // 步驟 3：資料遷移 - 為每位有配置的員工建立面板並指派 PanelId
            migrationBuilder.Sql(@"
                -- 為每位有 Shortcut 配置的員工建立「頁面捷徑」面板
                INSERT INTO EmployeeDashboardPanels (EmployeeId, Title, SortOrder, IconClass, Status, CreatedAt)
                SELECT DISTINCT EmployeeId, N'頁面捷徑', 0, 'bi bi-grid-fill', 1, GETDATE()
                FROM EmployeeDashboardConfigs
                WHERE SectionType = 'Shortcut';

                -- 為每位有 QuickAction 配置的員工建立「快速功能」面板
                INSERT INTO EmployeeDashboardPanels (EmployeeId, Title, SortOrder, IconClass, Status, CreatedAt)
                SELECT DISTINCT EmployeeId, N'快速功能', 1, 'bi bi-lightning-fill', 1, GETDATE()
                FROM EmployeeDashboardConfigs
                WHERE SectionType = 'QuickAction';

                -- 更新 Shortcut 配置的 PanelId
                UPDATE c
                SET c.PanelId = p.Id
                FROM EmployeeDashboardConfigs c
                INNER JOIN EmployeeDashboardPanels p ON c.EmployeeId = p.EmployeeId AND p.Title = N'頁面捷徑'
                WHERE c.SectionType = 'Shortcut';

                -- 更新 QuickAction 配置的 PanelId
                UPDATE c
                SET c.PanelId = p.Id
                FROM EmployeeDashboardConfigs c
                INNER JOIN EmployeeDashboardPanels p ON c.EmployeeId = p.EmployeeId AND p.Title = N'快速功能'
                WHERE c.SectionType = 'QuickAction';
            ");

            // 步驟 4：刪除沒有對應面板的孤兒記錄（如果有的話）
            migrationBuilder.Sql(@"
                DELETE FROM EmployeeDashboardConfigs WHERE PanelId IS NULL;
            ");

            // 步驟 5：移除舊索引
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDashboardConfigs_Employees_EmployeeId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_NavigationItemKey",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_SortOrder",
                table: "EmployeeDashboardConfigs");

            // 步驟 6：將 PanelId 設為 NOT NULL 並設定預設值
            migrationBuilder.AlterColumn<int>(
                name: "PanelId",
                table: "EmployeeDashboardConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 步驟 7：移除 SectionType 欄位
            migrationBuilder.DropColumn(
                name: "SectionType",
                table: "EmployeeDashboardConfigs");

            // 步驟 8：建立新索引與外鍵
            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId",
                table: "EmployeeDashboardConfigs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_NavigationItemKey",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "PanelId", "NavigationItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_SortOrder",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "PanelId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDashboardConfigs_EmployeeDashboardPanels_PanelId",
                table: "EmployeeDashboardConfigs",
                column: "PanelId",
                principalTable: "EmployeeDashboardPanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDashboardConfigs_Employees_EmployeeId",
                table: "EmployeeDashboardConfigs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDashboardConfigs_EmployeeDashboardPanels_PanelId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDashboardConfigs_Employees_EmployeeId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropTable(
                name: "EmployeeDashboardPanels");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_NavigationItemKey",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_SortOrder",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropColumn(
                name: "PanelId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.AddColumn<string>(
                name: "SectionType",
                table: "EmployeeDashboardConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_NavigationItemKey",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "EmployeeId", "NavigationItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_SortOrder",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "EmployeeId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDashboardConfigs_Employees_EmployeeId",
                table: "EmployeeDashboardConfigs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
