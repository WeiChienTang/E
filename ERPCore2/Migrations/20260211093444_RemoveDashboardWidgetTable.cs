using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDashboardWidgetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 先清空 EmployeeDashboardConfigs 表，因為舊資料結構不相容
            migrationBuilder.Sql("DELETE FROM EmployeeDashboardConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDashboardConfigs_DashboardWidgets_DashboardWidgetId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropTable(
                name: "DashboardWidgets");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_DashboardWidgetId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_DashboardWidgetId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropColumn(
                name: "DashboardWidgetId",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.AddColumn<string>(
                name: "NavigationItemKey",
                table: "EmployeeDashboardConfigs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_NavigationItemKey",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "EmployeeId", "NavigationItemKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_NavigationItemKey",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropColumn(
                name: "NavigationItemKey",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.AddColumn<int>(
                name: "DashboardWidgetId",
                table: "EmployeeDashboardConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DashboardWidgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultSortOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiredPermission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TargetRoute = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WidgetAction = table.Column<int>(type: "int", nullable: false),
                    WidgetCategory = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_DashboardWidgetId",
                table: "EmployeeDashboardConfigs",
                column: "DashboardWidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId_DashboardWidgetId",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "EmployeeId", "DashboardWidgetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_Code",
                table: "DashboardWidgets",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDashboardConfigs_DashboardWidgets_DashboardWidgetId",
                table: "EmployeeDashboardConfigs",
                column: "DashboardWidgetId",
                principalTable: "DashboardWidgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
