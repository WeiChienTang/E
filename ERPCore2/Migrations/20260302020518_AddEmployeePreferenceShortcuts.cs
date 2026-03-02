using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePreferenceShortcuts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortcutCalendar",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutPageSearch",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutQuickAction",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutReportSearch",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutStickyNotes",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortcutCalendar",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutPageSearch",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutQuickAction",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutReportSearch",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutStickyNotes",
                table: "EmployeePreferences");
        }
    }
}
